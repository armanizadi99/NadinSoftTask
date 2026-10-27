#pragma warning disable CS9113 // Dependencies are read once the handler is implemented.

using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Commands.RefreshTokens;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    ITokenService tokenService,
    IUnitOfWork unitOfWork) : IRequestHandler<RefreshTokenCommand, Result<AuthResult>>
{
    public Task<Result<AuthResult>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
