using CarRent.Application.Auth.Commands.UploadDriverLicense;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using CarRent.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Auth;

public class UploadDriverLicenseCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IImageUploaderService> _imageUploaderMock;
    private readonly Mock<IDriverLicenseVerifier> _verifierMock;
    private readonly UploadDriverLicenseCommandHandler _handler;

    public UploadDriverLicenseCommandHandlerTests()
    {
        _userManagerMock = TestHelper.CreateMockUserManager();
        _imageUploaderMock = new Mock<IImageUploaderService>();
        _verifierMock = new Mock<IDriverLicenseVerifier>();

        _userManagerMock
            .Setup(m => m.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        _handler = new UploadDriverLicenseCommandHandler(
            _userManagerMock.Object,
            _imageUploaderMock.Object,
            _verifierMock.Object);
    }

    private User SetupUser(DriverLicenseVerificationStatus status = DriverLicenseVerificationStatus.NotSubmitted)
    {
        User user = new() { Id = Guid.NewGuid(), LicenseStatus = status };
        _userManagerMock
            .Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        return user;
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        UploadDriverLicenseCommand command = new(Guid.NewGuid(), Stream.Null, "license.jpg", "ABC123456");
        _userManagerMock
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyVerified_ThrowsConflictException()
    {
        // Arrange
        User user = SetupUser(DriverLicenseVerificationStatus.Verified);
        UploadDriverLicenseCommand command = new(user.Id, Stream.Null, "license.jpg", "ABC123456");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenNumberInvalid_RejectsAndDoesNotUploadImage()
    {
        // Arrange
        User user = SetupUser();
        UploadDriverLicenseCommand command = new(user.Id, Stream.Null, "license.jpg", "12345");
        _verifierMock
            .Setup(v => v.VerifyAsync("12345", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        UploadDriverLicenseResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(DriverLicenseVerificationStatus.Rejected);
        user.LicenseStatus.Should().Be(DriverLicenseVerificationStatus.Rejected);
        _imageUploaderMock.Verify(
            u => u.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNumberValid_UploadsImageAndVerifiesUser()
    {
        // Arrange
        User user = SetupUser();
        UploadDriverLicenseCommand command = new(user.Id, Stream.Null, "license.jpg", "abc 123456");
        _verifierMock
            .Setup(v => v.VerifyAsync("ABC123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _imageUploaderMock
            .Setup(u => u.UploadAsync(It.IsAny<Stream>(), "license.jpg", It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://img.test/license.jpg");

        // Act
        UploadDriverLicenseResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(DriverLicenseVerificationStatus.Verified);
        user.DriverLicenseNumber.Should().Be("ABC123456");
        user.DriverLicenseImageUrl.Should().Be("https://img.test/license.jpg");
        user.LicenseStatus.Should().Be(DriverLicenseVerificationStatus.Verified);
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_ThrowsValidationException()
    {
        // Arrange
        User user = SetupUser();
        UploadDriverLicenseCommand command = new(user.Id, Stream.Null, "license.jpg", "ABC123456");
        _verifierMock
            .Setup(v => v.VerifyAsync("ABC123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _imageUploaderMock
            .Setup(u => u.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://img.test/license.jpg");
        _userManagerMock
            .Setup(m => m.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Update failed");
    }
}