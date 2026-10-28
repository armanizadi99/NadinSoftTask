using FluentValidation;
using NadinSoftTask.Application.Common;

namespace NadinSoftTask.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(command => command.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Username cannot be empty or whitespace.")
            .Length(UserConstraints.MinUsernameLength, UserConstraints.MaxUsernameLength)
            .WithMessage(
                $"Username length must be between {UserConstraints.MinUsernameLength} and {UserConstraints.MaxUsernameLength} characters.")
            .Matches(UserConstraints.UsernameAllowedCharactersPattern)
            .WithMessage("Username can only contain letters, numbers, and underscores.");

        RuleFor(command => command.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Password cannot be empty or whitespace.")
            .Length(UserConstraints.MinPasswordLength, UserConstraints.MaxPasswordLength)
            .WithMessage(
                $"Password length must be between {UserConstraints.MinPasswordLength} and {UserConstraints.MaxPasswordLength} characters.");
    }
}
