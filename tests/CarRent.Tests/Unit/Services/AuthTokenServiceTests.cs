using AutoMapper;
using CarRent.Application.Common.Settings;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Services;
using CarRent.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Services;

public class AuthTokenServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IJwtTokenGenerator> _jwtGeneratorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly IMapper _mapper;
    private readonly IOptions<JwtSettings> _jwtOptions;
    private readonly AuthTokenService _service;

    public AuthTokenServiceTests()
    {
        _userManagerMock = TestHelper.CreateMockUserManager();
        _jwtGeneratorMock = new Mock<IJwtTokenGenerator>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepoMock.Object);
        _mapper = TestHelper.CreateMapper();

        JwtSettings jwtSettings = new()
        {
            SecretKey = "super-secret-key-that-is-long-enough-for-hmac-sha256",
            Issuer = "CarRentApi",
            Audience = "CarRentClient",
            ExpiryDays = 7,
            RefreshTokenExpiryDays = 30
        };
        _jwtOptions = Options.Create(jwtSettings);

        _service = new AuthTokenService(
            _userManagerMock.Object,
            _jwtGeneratorMock.Object,
            _unitOfWorkMock.Object,
            _mapper,
            _jwtOptions);
    }

    [Fact]
    public async Task IssueAsync_WhenCalled_RevokesExistingActiveTokensAndCreatesNewOne()
    {
        // Arrange
        User user = TestHelper.CreateTestUser();
        List<string> roles = new() { "User" };

        RefreshToken oldToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "old-refresh-token",
            IsRevoked = false,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(5)
        };

        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(roles);
        _jwtGeneratorMock.Setup(g => g.GenerateAccessToken(user, roles)).Returns("jwt-access-token");
        _jwtGeneratorMock.Setup(g => g.GenerateRefreshToken()).Returns("new-refresh-token");
        _refreshTokenRepoMock.Setup(r => r.GetActiveByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken> { oldToken });

        RefreshToken? addedToken = null;
        _refreshTokenRepoMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((t, _) => addedToken = t)
            .Returns(Task.CompletedTask);

        // Act
        AuthResponse result = await _service.IssueAsync(user, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("jwt-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        result.User.Email.Should().Be(user.Email);
        result.User.Roles.Should().Contain("User");

        oldToken.IsRevoked.Should().BeTrue();
        oldToken.ReplacedByToken.Should().Be("new-refresh-token");

        addedToken.Should().NotBeNull();
        addedToken!.UserId.Should().Be(user.Id);
        addedToken.Token.Should().Be("new-refresh-token");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
