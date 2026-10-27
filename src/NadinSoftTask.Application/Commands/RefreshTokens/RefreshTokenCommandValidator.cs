using FluentValidation;

namespace NadinSoftTask.Application.Commands.RefreshTokens;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.RefreshToken)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Refresh token cannot be empty or whitespace.");
    }
}
