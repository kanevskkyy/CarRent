using AutoMapper;
using CarRent.Application.Cars.Commands.UpdateCar;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using CarRent.Tests.Common;
using FluentAssertions;
using Moq;
using Xunit;
using DomainDriveType = CarRent.Domain.Enums.DriveType;

namespace CarRent.Tests.Unit.Handlers.Cars;

public class UpdateCarCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICarRepository> _carRepoMock;
    private readonly Mock<IImageUploaderService> _imageUploaderMock;
    private readonly IMapper _mapper;
    private readonly UpdateCarCommandHandler _handler;

    public UpdateCarCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _carRepoMock = new Mock<ICarRepository>();
        _unitOfWorkMock.Setup(u => u.Cars).Returns(_carRepoMock.Object);
        _imageUploaderMock = new Mock<IImageUploaderService>();
        _mapper = TestHelper.CreateMapper();

        _handler = new UpdateCarCommandHandler(
            _unitOfWorkMock.Object,
            _imageUploaderMock.Object,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenCarNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid carId = Guid.NewGuid();
        UpdateCarRequest request = new() { Name = "Updated Name" };
        UpdateCarCommand command = new(carId, request);

        _carRepoMock.Setup(c => c.GetByIdAsync(carId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Car?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{carId}*");
    }

    [Fact]
    public async Task Handle_WhenValidWithoutNewPhoto_UpdatesPropertiesAndRetainsOldPhoto()
    {
        // Arrange
        Car existingCar = TestHelper.CreateTestCar(name: "Old Name", dailyPrice: 80m);
        string originalPhoto = existingCar.PhotoUrl;

        UpdateCarRequest request = new()
        {
            Name = "Updated Name",
            DailyPrice = 95m,
            Year = existingCar.Year,
            BodyType = existingCar.BodyType,
            EngineType = existingCar.EngineType,
            DriveType = existingCar.DriveType,
            TransmissionType = existingCar.TransmissionType,
            HorsePower = existingCar.HorsePower,
            SeatsCount = existingCar.SeatsCount,
            TrunkVolumeLiters = existingCar.TrunkVolumeLiters
        };
        UpdateCarCommand command = new(existingCar.Id, request);

        _carRepoMock.Setup(c => c.GetByIdAsync(existingCar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCar);

        // Act
        CarDetailResponse? result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.DailyPrice.Should().Be(95m);
        existingCar.PhotoUrl.Should().Be(originalPhoto);
        _imageUploaderMock.Verify(u => u.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _carRepoMock.Verify(c => c.Update(existingCar), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNewPhotoProvided_UploadsAndUpdatesPhotoUrl()
    {
        // Arrange
        Car existingCar = TestHelper.CreateTestCar(name: "Old Name");
        UpdateCarRequest request = new()
        {
            Name = "Updated Name",
            DailyPrice = 110m,
            Year = existingCar.Year,
            BodyType = existingCar.BodyType,
            EngineType = existingCar.EngineType,
            DriveType = existingCar.DriveType,
            TransmissionType = existingCar.TransmissionType,
            HorsePower = existingCar.HorsePower,
            SeatsCount = existingCar.SeatsCount,
            TrunkVolumeLiters = existingCar.TrunkVolumeLiters
        };

        using MemoryStream stream = new(new byte[] { 4, 5, 6 });
        string fileName = "newphoto.jpg";
        string newUrl = "https://cdn.example.com/newphoto.jpg";

        UpdateCarCommand command = new(existingCar.Id, request, stream, fileName);

        _carRepoMock.Setup(c => c.GetByIdAsync(existingCar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCar);
        _imageUploaderMock.Setup(u => u.UploadAsync(stream, fileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUrl);

        // Act
        CarDetailResponse? result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PhotoUrl.Should().Be(newUrl);
        existingCar.PhotoUrl.Should().Be(newUrl);
        _imageUploaderMock.Verify(u => u.UploadAsync(stream, fileName, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
