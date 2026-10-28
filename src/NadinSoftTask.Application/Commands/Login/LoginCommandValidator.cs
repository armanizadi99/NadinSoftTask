using FluentValidation;

namespace NadinSoftTask.Application.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Username)
            .NotEmpty()
            .WithMessage("Username cannot be empty or whitespace.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("Password cannot be empty or whitespace.");
    }
}
