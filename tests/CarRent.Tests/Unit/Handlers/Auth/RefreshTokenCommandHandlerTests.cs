using CarRent.Application.Auth.Commands.RefreshToken;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using CarRent.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly Mock<IAuthTokenService> _tokenServiceMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _userManagerMock = TestHelper.CreateMockUserManager();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepoMock.Object);
        _tokenServiceMock = new Mock<IAuthTokenService>();

        _handler = new RefreshTokenCommandHandler(
            _userManagerMock.Object,
            _unitOfWorkMock.Object,
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTokenNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        RefreshTokenCommand command = new(new RefreshTokenRequest { RefreshToken = "invalid-token" });
        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(command.Request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task Handle_WhenTokenIsExpiredOrRevoked_ThrowsUnauthorizedException()
    {
        // Arrange
        RefreshToken expiredToken = new()
        {
            Token = "expired-token",
            ExpiresOnUtc = DateTime.UtcNow.AddMinutes(-5),
            IsRevoked = false
        };
        RefreshTokenCommand command = new(new RefreshTokenRequest { RefreshToken = expiredToken.Token });

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(command.Request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        RefreshToken validToken = new()
        {
            Token = "valid-token",
            UserId = userId,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        RefreshTokenCommand command = new(new RefreshTokenRequest { RefreshToken = validToken.Token });

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(command.Request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validToken);
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User associated with token no longer exists.");
    }

    [Fact]
    public async Task Handle_WhenTokenAndUserValid_ReturnsNewAuthResponse()
    {
        // Arrange
        User user = TestHelper.CreateTestUser();
        RefreshToken validToken = new()
        {
            Token = "valid-token",
            UserId = user.Id,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        RefreshTokenCommand command = new(new RefreshTokenRequest { RefreshToken = validToken.Token });
        AuthResponse expectedAuth = new()
        {
            AccessToken = "new-jwt",
            RefreshToken = "new-refresh",
            User = new UserResponse { Id = user.Id, Email = user.Email! }
        };

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(command.Request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validToken);
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(t => t.IssueAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAuth);

        // Act
        AuthResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new-jwt");
        result.RefreshToken.Should().Be("new-refresh");
    }
}
