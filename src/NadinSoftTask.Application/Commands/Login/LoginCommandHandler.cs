using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Users;

namespace NadinSoftTask.Application.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<LoginCommand, Result<AuthResult>>
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

        (string accessToken, DateTime accessTokenExpiresAt) =
            tokenService.GenerateAccessToken(user.Id, user.Username);

        string refreshTokenValue = tokenService.GenerateRefreshToken();
        DateTime refreshTokenExpiresAt = tokenService.GetRefreshTokenExpiration();
        RefreshToken refreshToken = new(refreshTokenValue, user.Id, refreshTokenExpiresAt);

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            accessToken,
            refreshTokenValue,
            accessTokenExpiresAt,
            refreshTokenExpiresAt,
            user.Id,
            user.Username);
    }

    private static DomainError InvalidCredentials() =>
        new("Invalid username or password.", DomainErrorKind.NotAuthorized);
}
