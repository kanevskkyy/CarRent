using CarRent.Application.DTOs.Auth;
using FluentValidation;

namespace CarRent.Application.Validators.Auth;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.")
            .MinimumLength(10).WithMessage("Refresh token has an invalid format.")
            .MaximumLength(512).WithMessage("Refresh token exceeds maximum allowed length.");
    }
}
