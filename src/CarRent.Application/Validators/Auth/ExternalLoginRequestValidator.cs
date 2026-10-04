using CarRent.Application.DTOs.Auth;
using FluentValidation;

namespace CarRent.Application.Validators.Auth;

public class ExternalLoginRequestValidator : AbstractValidator<ExternalLoginRequest>
{
    public ExternalLoginRequestValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("IdToken is required.")
            .MaximumLength(4096).WithMessage("IdToken is too long.");
    }
}