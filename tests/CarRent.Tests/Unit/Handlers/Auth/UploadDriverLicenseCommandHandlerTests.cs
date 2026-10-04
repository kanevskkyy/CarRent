using CarRent.Application.Auth.Commands.UploadDriverLicense;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using CarRent.Tests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Auth;

public class UploadDriverLicenseCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCalled_ThrowsNotImplementedException()
    {
        // Arrange
        Mock<UserManager<User>> userManagerMock = TestHelper.CreateMockUserManager();
        Mock<IImageUploaderService> imageUploaderMock = new();
        Mock<IUnitOfWork> unitOfWorkMock = new();

        UploadDriverLicenseCommandHandler handler = new(
            userManagerMock.Object,
            imageUploaderMock.Object,
            unitOfWorkMock.Object);

        UploadDriverLicenseCommand command = new(Guid.NewGuid(), Stream.Null, "license.jpg");

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("UploadDriverLicense is not yet implemented.");
    }
}
