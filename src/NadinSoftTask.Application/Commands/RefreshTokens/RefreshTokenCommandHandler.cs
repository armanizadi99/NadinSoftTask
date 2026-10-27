using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Users;

namespace NadinSoftTask.Application.Commands.RefreshTokens;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    ITokenService tokenService,
    IUnitOfWork unitOfWork) : IRequestHandler<RefreshTokenCommand, Result<AuthResult>>
{
    public async Task<Result<AuthResult>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        RefreshToken? storedRefreshToken = await refreshTokenRepository.GetByTokenAsync(
            request.RefreshToken,
            cancellationToken);

        if (storedRefreshToken is null || !storedRefreshToken.IsActive(DateTime.UtcNow))
        {
            return InvalidRefreshToken();
        }

        UserDto? user = await userRepository.GetByIdAsync(
            storedRefreshToken.UserId,
            cancellationToken);

        if (user is null)
        {
            return InvalidRefreshToken();
        }

        storedRefreshToken.Revoke();

        (string accessToken, DateTime accessTokenExpiresAt) =
            tokenService.GenerateAccessToken(user.Id, user.Username);

        string refreshTokenValue = tokenService.GenerateRefreshToken();
        DateTime refreshTokenExpiresAt = tokenService.GetRefreshTokenExpiration();
        RefreshToken newRefreshToken = new(refreshTokenValue, user.Id, refreshTokenExpiresAt);

        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            accessToken,
            refreshTokenValue,
            accessTokenExpiresAt,
            refreshTokenExpiresAt,
            user.Id,
            user.Username);
    }

    private static DomainError InvalidRefreshToken() =>
        new("Invalid refresh token.", DomainErrorKind.NotAuthorized);
}
