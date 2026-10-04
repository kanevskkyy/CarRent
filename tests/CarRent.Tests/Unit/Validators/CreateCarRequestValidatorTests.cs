using CarRent.Application.DTOs.Cars;
using CarRent.Application.Validators.Cars;
using CarRent.Domain.Enums;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using DomainDriveType = CarRent.Domain.Enums.DriveType;

namespace CarRent.Tests.Unit.Validators;

public class CreateCarRequestValidatorTests
{
    private readonly CreateCarRequestValidator _validator = new();

    private static IFormFile CreateMockFormFile(string fileName = "car.jpg", long length = 1024)
    {
        Mock<IFormFile> mock = new();
        mock.Setup(f => f.FileName).Returns(fileName);
        mock.Setup(f => f.Length).Returns(length);
        return mock.Object;
    }

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldNotHaveErrors()
    {
        // Arrange
        CreateCarRequest request = new()
        {
            Name = "Toyota Camry",
            Year = 2022,
            DailyPrice = 75m,
            BodyType = BodyType.Sedan,
            EngineType = EngineType.Petrol,
            DriveType = DomainDriveType.FrontWheel,
            TransmissionType = TransmissionType.Automatic,
            HorsePower = 203,
            SeatsCount = 5,
            TrunkVolumeLiters = 450,
            Photo = CreateMockFormFile("camry.jpg")
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldHaveError()
    {
        // Arrange
        CreateCarRequest request = new()
        {
            Name = "",
            Year = 2022,
            DailyPrice = 75m,
            BodyType = BodyType.Sedan,
            EngineType = EngineType.Petrol,
            DriveType = DomainDriveType.FrontWheel,
            TransmissionType = TransmissionType.Automatic,
            HorsePower = 203,
            SeatsCount = 5,
            TrunkVolumeLiters = 450,
            Photo = CreateMockFormFile()
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCarRequest.Name));
    }

    [Fact]
    public void Validate_WhenDailyPriceIsZeroOrNegative_ShouldHaveError()
    {
        // Arrange
        CreateCarRequest request = new()
        {
            Name = "Camry",
            Year = 2022,
            DailyPrice = -10m,
            BodyType = BodyType.Sedan,
            EngineType = EngineType.Petrol,
            DriveType = DomainDriveType.FrontWheel,
            TransmissionType = TransmissionType.Automatic,
            HorsePower = 203,
            SeatsCount = 5,
            TrunkVolumeLiters = 450,
            Photo = CreateMockFormFile()
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCarRequest.DailyPrice));
    }

    [Fact]
    public void Validate_WhenPhotoHasInvalidExtension_ShouldHaveError()
    {
        // Arrange
        CreateCarRequest request = new()
        {
            Name = "Camry",
            Year = 2022,
            DailyPrice = 50m,
            BodyType = BodyType.Sedan,
            EngineType = EngineType.Petrol,
            DriveType = DomainDriveType.FrontWheel,
            TransmissionType = TransmissionType.Automatic,
            HorsePower = 203,
            SeatsCount = 5,
            TrunkVolumeLiters = 450,
            Photo = CreateMockFormFile("script.exe")
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCarRequest.Photo));
    }
}
