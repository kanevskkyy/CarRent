using CarRent.Application.Auth.Commands.ExternalLogin;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Models.Auth;
using CarRent.Domain.Entities;
using CarRent.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Auth;

public class ExternalLoginCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IExternalAuthProvider> _googleProviderMock;
    private readonly Mock<IAuthTokenService> _tokenServiceMock;
    private readonly ExternalLoginCommandHandler _handler;

    public ExternalLoginCommandHandlerTests()
    {
        _userManagerMock = TestHelper.CreateMockUserManager();
        _googleProviderMock = new Mock<IExternalAuthProvider>();
        _googleProviderMock.Setup(p => p.Provider).Returns("google");
        _tokenServiceMock = new Mock<IAuthTokenService>();

        _handler = new ExternalLoginCommandHandler(
            _userManagerMock.Object,
            new[] { _googleProviderMock.Object },
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenProviderNotSupported_ThrowsValidationException()
    {
        // Arrange
        ExternalLoginCommand command = new("facebook", new ExternalLoginRequest { IdToken = "token" });

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Provider 'facebook' is not supported.*");
    }

    [Fact]
    public async Task Handle_WhenExternalAccountHasUnverifiedEmail_ThrowsUnauthorizedException()
    {
        // Arrange
        ExternalLoginCommand command = new("google", new ExternalLoginRequest { IdToken = "token" });
        _googleProviderMock.Setup(p => p.ValidateAsync("token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExternalUserInfo("google", "sub123", "unverified@test.com", false, null));

        _userManagerMock.Setup(m => m.FindByLoginAsync("google", "sub123"))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*account has no verified email.*");
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyLinked_ReturnsAuthResponse()
    {
        // Arrange
        ExternalLoginCommand command = new("google", new ExternalLoginRequest { IdToken = "valid-token" });
        User existingUser = TestHelper.CreateTestUser();
        AuthResponse expectedAuth = new() { AccessToken = "tok", RefreshToken = "ref" };

        _googleProviderMock.Setup(p => p.ValidateAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExternalUserInfo("google", "sub123", existingUser.Email, true, null));

        _userManagerMock.Setup(m => m.FindByLoginAsync("google", "sub123"))
            .ReturnsAsync(existingUser);
        _tokenServiceMock.Setup(t => t.IssueAsync(existingUser, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAuth);

        // Act
        AuthResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedAuth);
        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserExistsByEmailButNotLinked_LinksLoginAndReturnsAuth()
    {
        // Arrange
        ExternalLoginCommand command = new("google", new ExternalLoginRequest { IdToken = "valid-token" });
        User existingUser = TestHelper.CreateTestUser(email: "exist@test.com");
        AuthResponse expectedAuth = new() { AccessToken = "tok", RefreshToken = "ref" };

        _googleProviderMock.Setup(p => p.ValidateAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExternalUserInfo("google", "sub123", "exist@test.com", true, null));

        _userManagerMock.Setup(m => m.FindByLoginAsync("google", "sub123"))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByEmailAsync("exist@test.com"))
            .ReturnsAsync(existingUser);
        _userManagerMock.Setup(m => m.AddLoginAsync(existingUser, It.IsAny<UserLoginInfo>()))
            .ReturnsAsync(IdentityResult.Success);
        _tokenServiceMock.Setup(t => t.IssueAsync(existingUser, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAuth);

        // Act
        AuthResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedAuth);
        _userManagerMock.Verify(m => m.AddLoginAsync(existingUser, It.Is<UserLoginInfo>(l => l.LoginProvider == "google" && l.ProviderKey == "sub123")), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNewUser_CreatesUserAddsRoleLinksLoginAndReturnsAuth()
    {
        // Arrange
        ExternalLoginCommand command = new("google", new ExternalLoginRequest { IdToken = "valid-token" });
        AuthResponse expectedAuth = new() { AccessToken = "tok", RefreshToken = "ref" };

        _googleProviderMock.Setup(p => p.ValidateAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExternalUserInfo("google", "sub123", "brandnew@test.com", true, "Brand New"));

        _userManagerMock.Setup(m => m.FindByLoginAsync("google", "sub123"))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByEmailAsync("brandnew@test.com"))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "User"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddLoginAsync(It.IsAny<User>(), It.IsAny<UserLoginInfo>()))
            .ReturnsAsync(IdentityResult.Success);
        _tokenServiceMock.Setup(t => t.IssueAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAuth);

        // Act
        AuthResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedAuth);
        _userManagerMock.Verify(m => m.CreateAsync(It.Is<User>(u => u.Email == "brandnew@test.com")), Times.Once);
        _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<User>(), "User"), Times.Once);
    }
}
