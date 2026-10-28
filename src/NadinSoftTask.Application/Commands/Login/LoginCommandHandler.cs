using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService) : IRequestHandler<LoginCommand, Result<AuthResult>>
{
    public async Task<Result<AuthResult>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        UserDto? user = await userRepository.GetByUsernameAsync(
            request.Username,
            cancellationToken);

        if (user is null)
        {
            return InvalidCredentials();
        }

        bool passwordIsValid = await userRepository.CheckPasswordAsync(
            user.Id,
            request.Password,
            cancellationToken);

        if (!passwordIsValid)
        {
            return InvalidCredentials();
        }

        return await tokenService.GenerateTokensAsync(
            user.Id,
            user.Username,
            cancellationToken);
    }

    private static DomainError InvalidCredentials() =>
        new("Invalid username or password.", DomainErrorKind.NotAuthorized);
}
