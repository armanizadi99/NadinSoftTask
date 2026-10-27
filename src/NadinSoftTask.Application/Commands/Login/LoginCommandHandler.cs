#pragma warning disable CS9113 // Dependencies are read once the handler is implemented.

using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<LoginCommand, Result<AuthResult>>
{
    public Task<Result<AuthResult>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
