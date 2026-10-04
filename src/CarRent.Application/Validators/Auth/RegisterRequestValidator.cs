using CarRent.Application.DTOs.Auth;
using FluentValidation;

namespace CarRent.Application.Validators.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    private static readonly string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$";

    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters.")
            .Matches(PasswordPattern)
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters long.")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.")
            .Matches(@"^[\p{L}\s\-']+$").WithMessage("Full name can only contain letters, spaces, hyphens and apostrophes.");

        RuleFor(x => x.DriverLicenseNumber)
            .MaximumLength(50).WithMessage("Driver license number cannot exceed 50 characters.")
            .Matches(@"^[A-Za-z0-9\-]+$").WithMessage("Driver license number can only contain letters, digits and hyphens.")
            .When(x => !string.IsNullOrEmpty(x.DriverLicenseNumber));
    }
}
