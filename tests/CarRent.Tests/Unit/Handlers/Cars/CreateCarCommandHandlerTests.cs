using AutoMapper;
using CarRent.Application.Cars.Commands.CreateCar;
using CarRent.Application.DTOs.Cars;
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

public class CreateCarCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICarRepository> _carRepoMock;
    private readonly Mock<IImageUploaderService> _imageUploaderMock;
    private readonly IMapper _mapper;
    private readonly CreateCarCommandHandler _handler;

    public CreateCarCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _carRepoMock = new Mock<ICarRepository>();
        _unitOfWorkMock.Setup(u => u.Cars).Returns(_carRepoMock.Object);
        _imageUploaderMock = new Mock<IImageUploaderService>();
        _mapper = TestHelper.CreateMapper();

        _handler = new CreateCarCommandHandler(
            _unitOfWorkMock.Object,
            _imageUploaderMock.Object,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_UploadsPhotoAndSavesCar()
    {
        // Arrange
        CreateCarRequest request = new()
        {
            Name = "Audi A6",
            Year = 2023,
            DailyPrice = 120m,
            BodyType = BodyType.Sedan,
            EngineType = EngineType.Petrol,
            DriveType = DomainDriveType.AllWheel,
            TransmissionType = TransmissionType.Automatic,
            HorsePower = 250,
            SeatsCount = 5,
            TrunkVolumeLiters = 530
        };

        using MemoryStream stream = new(new byte[] { 1, 2, 3 });
        string fileName = "audi.jpg";
        CreateCarCommand command = new(request, stream, fileName);

        string uploadedUrl = "https://cdn.example.com/audi.jpg";
        _imageUploaderMock.Setup(u => u.UploadAsync(stream, fileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadedUrl);

        Car? addedCar = null;
        _carRepoMock.Setup(c => c.AddAsync(It.IsAny<Car>(), It.IsAny<CancellationToken>()))
            .Callback<Car, CancellationToken>((c, _) => addedCar = c)
            .Returns(Task.CompletedTask);

        // Act
        CarDetailResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Audi A6");
        result.PhotoUrl.Should().Be(uploadedUrl);

        addedCar.Should().NotBeNull();
        addedCar!.Name.Should().Be("Audi A6");
        addedCar.PhotoUrl.Should().Be(uploadedUrl);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
