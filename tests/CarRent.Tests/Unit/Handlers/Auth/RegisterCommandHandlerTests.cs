using CarRent.Application.Auth.Commands.Register;
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

public class RegisterCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IAuthTokenService> _tokenServiceMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userManagerMock = TestHelper.CreateMockUserManager();
        _tokenServiceMock = new Mock<IAuthTokenService>();
        _handler = new RegisterCommandHandler(_userManagerMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ThrowsConflictException()
    {
        // Arrange
        RegisterRequest request = new()
        {
            Email = "existing@example.com",
            FullName = "Existing User",
            Password = "Password123!",
            DriverLicenseNumber = "DL-999"
        };
        RegisterCommand command = new(request);
        User existingUser = TestHelper.CreateTestUser(email: request.Email);

        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage($"*{request.Email}*");
    }

    [Fact]
    public async Task Handle_WhenCreationFails_ThrowsValidationException()
    {
        // Arrange
        RegisterRequest request = new()
        {
            Email = "new@example.com",
            FullName = "New User",
            Password = "weak",
            DriverLicenseNumber = "DL-111"
        };
        RegisterCommand command = new(request);

        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too short." }));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Password too short.*");
    }

    [Fact]
    public async Task Handle_WhenValidRegistration_CreatesUserAndReturnsTokens()
    {
        // Arrange
        RegisterRequest request = new()
        {
            Email = "new@example.com",
            FullName = "New User",
            Password = "SecurePassword123!",
            DriverLicenseNumber = "DL-123456"
        };
        RegisterCommand command = new(request);
        AuthResponse authResponse = new()
        {
            AccessToken = "jwt-access",
            RefreshToken = "refresh-val",
            User = new UserResponse { Email = request.Email, FullName = request.FullName }
        };

        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "User"))
            .ReturnsAsync(IdentityResult.Success);
        _tokenServiceMock.Setup(t => t.IssueAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        AuthResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("jwt-access");
        result.RefreshToken.Should().Be("refresh-val");
        _userManagerMock.Verify(m => m.AddToRoleAsync(It.Is<User>(u => u.Email == request.Email), "User"), Times.Once);
    }
}
