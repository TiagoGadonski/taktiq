using FluentValidation;
using GymHero.Application.Features.Auth.Commands;

namespace GymHero.Application.Features.Auth.Validators;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain one number.");
    }
}