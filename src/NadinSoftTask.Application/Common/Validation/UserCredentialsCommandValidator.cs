using FluentValidation;
using NadinSoftTask.Application.Common.Abstractions;

namespace NadinSoftTask.Application.Common.Validation;

public abstract class UserCredentialsCommandValidator<T> : AbstractValidator<T>
    where T : IUserCredentialsCommand
{
    protected UserCredentialsCommandValidator()
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
