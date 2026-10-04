using CarRent.Application.DTOs.Cars;
using FluentValidation;

namespace CarRent.Application.Validators.Cars;

public class CreateCarRequestValidator : AbstractValidator<CreateCarRequest>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024;

    public CreateCarRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Car name is required.")
            .MaximumLength(150).WithMessage("Car name must not exceed 150 characters.");

        RuleFor(x => x.Year)
            .NotEmpty().WithMessage("Year is required.")
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage($"Year must be between 1900 and {DateTime.UtcNow.Year + 1}.");

        RuleFor(x => x.DailyPrice)
            .GreaterThan(0).WithMessage("Daily price must be greater than zero.");

        RuleFor(x => x.BodyType)
            .IsInEnum().WithMessage("Invalid body type.");

        RuleFor(x => x.EngineType)
            .IsInEnum().WithMessage("Invalid engine type.");

        RuleFor(x => x.DriveType)
            .IsInEnum().WithMessage("Invalid drive type.");

        RuleFor(x => x.TransmissionType)
            .IsInEnum().WithMessage("Invalid transmission type.");

        RuleFor(x => x.HorsePower)
            .GreaterThan(0).WithMessage("Horsepower must be greater than zero.")
            .LessThanOrEqualTo(2000).WithMessage("Horsepower must be realistic.");

        RuleFor(x => x.SeatsCount)
            .InclusiveBetween(1, 100).WithMessage("Seats count must be between 1 and 100.");

        RuleFor(x => x.TrunkVolumeLiters)
            .GreaterThanOrEqualTo(0).WithMessage("Trunk volume cannot be negative.");

        RuleFor(x => x.Photo)
            .NotNull().WithMessage("Car photo is required.")
            .Must(file => file != null && file.Length <= MaxFileSizeInBytes)
            .WithMessage("File size must not exceed 5MB.")
            .Must(file => file != null && AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
            .WithMessage("Only image files (.jpg, .jpeg, .png, .webp) are allowed.");
    }
}