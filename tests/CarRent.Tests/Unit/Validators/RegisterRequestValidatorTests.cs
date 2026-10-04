using CarRent.Application.DTOs.Auth;
using CarRent.Application.Validators.Auth;
using FluentAssertions;
using FluentValidation.Results;
using Xunit;

namespace CarRent.Tests.Unit.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldNotHaveErrors()
    {
        // Arrange
        RegisterRequest request = new()
        {
            Email = "john.doe@example.com",
            FullName = "John Doe",
            Password = "SecurePassword123!",
            DriverLicenseNumber = "DL-12345"
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenEmailIsInvalid_ShouldHaveError()
    {
        // Arrange
        RegisterRequest request = new()
        {
            Email = "not-an-email",
            FullName = "John Doe",
            Password = "SecurePassword123!"
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Email));
    }

    [Theory]
    [InlineData("short")] // < 8
    [InlineData("alllowercase123!")] // no uppercase
    [InlineData("ALLUPPERCASE123!")] // no lowercase
    [InlineData("NoSpecialChars123")] // no special char
    [InlineData("NoDigitsInPassword!")] // no digit
    public void Validate_WhenPasswordDoesNotMeetRequirements_ShouldHaveError(string password)
    {
        // Arrange
        RegisterRequest request = new()
        {
            Email = "valid@example.com",
            FullName = "John Doe",
            Password = password
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void Validate_WhenFullNameContainsInvalidCharacters_ShouldHaveError()
    {
        // Arrange
        RegisterRequest request = new()
        {
            Email = "valid@example.com",
            FullName = "John Doe 123", // digits not allowed by regex
            Password = "SecurePassword123!"
        };

        // Act
        ValidationResult result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.FullName));
    }
}
