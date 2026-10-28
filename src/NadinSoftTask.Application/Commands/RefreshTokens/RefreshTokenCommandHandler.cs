using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Commands.RefreshTokens;

public sealed class RefreshTokenCommandHandler(ITokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, Result<AuthResult>>
{
    public Task<Result<AuthResult>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken) =>
        tokenService.RefreshTokensAsync(request.RefreshToken, cancellationToken);
}
