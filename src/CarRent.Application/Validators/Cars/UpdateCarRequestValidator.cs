using CarRent.Application.DTOs.Cars;
using FluentValidation;

namespace CarRent.Application.Validators.Cars;

public class UpdateCarRequestValidator : AbstractValidator<UpdateCarRequest>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024;

    public UpdateCarRequestValidator()
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

        When(x => x.Photo != null, () =>
        {
            RuleFor(x => x.Photo!)
                .Must(file => file.Length <= MaxFileSizeInBytes)
                .WithMessage("File size must not exceed 5MB.")
                .Must(file => AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                .WithMessage("Only image files (.jpg, .jpeg, .png, .webp) are allowed.");
        });
    }
}