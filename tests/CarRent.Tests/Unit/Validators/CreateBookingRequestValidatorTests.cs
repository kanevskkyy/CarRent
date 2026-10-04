using CarRent.Application.DTOs.Bookings;
using CarRent.Application.Validators.Bookings;
using FluentAssertions;
using FluentValidation.Results;
using Xunit;

namespace CarRent.Tests.Unit.Validators;

public class CreateBookingRequestValidatorTests
{
    private readonly CreateBookingRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldNotHaveErrors()
    {
        // Arrange
        CreateBookingRequest request = new()
        {
            CarId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.Date.AddDays(1),
            EndDate = DateTime.UtcNow.Date.AddDays(3)
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenCarIdIsEmpty_ShouldHaveError()
    {
        // Arrange
        CreateBookingRequest request = new()
        {
            CarId = Guid.Empty,
            StartDate = DateTime.UtcNow.Date.AddDays(1),
            EndDate = DateTime.UtcNow.Date.AddDays(3)
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingRequest.CarId));
    }

    [Fact]
    public void Validate_WhenStartDateIsInThePast_ShouldHaveError()
    {
        // Arrange
        CreateBookingRequest request = new()
        {
            CarId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.Date.AddDays(-1),
            EndDate = DateTime.UtcNow.Date.AddDays(2)
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingRequest.StartDate));
    }

    [Fact]
    public void Validate_WhenEndDateIsBeforeStartDate_ShouldHaveError()
    {
        // Arrange
        CreateBookingRequest request = new()
        {
            CarId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.Date.AddDays(5),
            EndDate = DateTime.UtcNow.Date.AddDays(2)
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBookingRequest.EndDate));
    }
}
