using NadinSoftTask.Application.Common.Validation;

namespace NadinSoftTask.Application.Commands.Login;

public sealed class LoginCommandValidator
    : UserCredentialsCommandValidator<LoginCommand>
{
}
