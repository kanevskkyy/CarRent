using CarRent.Application.Auth.Commands.Login;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using CarRent.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IAuthTokenService> _tokenServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userManagerMock = TestHelper.CreateMockUserManager();
        _tokenServiceMock = new Mock<IAuthTokenService>();
        _handler = new LoginCommandHandler(_userManagerMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsUnauthorizedException()
    {
        // Arrange
        LoginCommand command = new(new LoginRequest { Email = "nonexistent@test.com", Password = "Password123!" });
        _userManagerMock.Setup(m => m.FindByEmailAsync(command.Request.Email))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_WhenPasswordIsIncorrect_ThrowsUnauthorizedException()
    {
        // Arrange
        User user = TestHelper.CreateTestUser();
        LoginCommand command = new(new LoginRequest { Email = user.Email!, Password = "WrongPassword!" });

        _userManagerMock.Setup(m => m.FindByEmailAsync(command.Request.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, command.Request.Password))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ReturnsAuthResponse()
    {
        // Arrange
        User user = TestHelper.CreateTestUser();
        LoginCommand command = new(new LoginRequest { Email = user.Email!, Password = "CorrectPassword123!" });
        AuthResponse expectedResponse = new()
        {
            AccessToken = "valid-access-token",
            RefreshToken = "valid-refresh-token",
            RefreshTokenExpiresOn = DateTime.UtcNow.AddDays(7),
            User = new UserResponse { Id = user.Id, Email = user.Email! }
        };

        _userManagerMock.Setup(m => m.FindByEmailAsync(command.Request.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, command.Request.Password))
            .ReturnsAsync(true);
        _tokenServiceMock.Setup(t => t.IssueAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        AuthResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(expectedResponse.AccessToken);
        result.RefreshToken.Should().Be(expectedResponse.RefreshToken);
        result.User.Email.Should().Be(user.Email);
    }
}
