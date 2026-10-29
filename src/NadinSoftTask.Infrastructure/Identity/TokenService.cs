using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NadinSoftTask.Application.Common;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Application.Common.Security;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Infrastructure.Common;

namespace NadinSoftTask.Infrastructure.Identity;

public sealed class TokenService(
    IRefreshTokenStore refreshTokenStore,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IOptions<JwtOptions> options,
    TimeProvider timeProvider) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public async Task<Result<AuthResult>> GenerateTokensAsync(
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;

        (string accessToken, DateTime accessTokenExpiresAt) =
            GenerateAccessToken(userId, userName, now);

        string refreshToken = GenerateRefreshToken();
        DateTime refreshTokenExpiresAt = GetRefreshTokenExpiration(now);

        RefreshToken storedRefreshToken = new(refreshToken, userId, refreshTokenExpiresAt, now);

        await refreshTokenStore.AddAsync(storedRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            accessToken,
            refreshToken,
            accessTokenExpiresAt,
            refreshTokenExpiresAt,
            userId,
            userName);
    }

    public async Task<Result<AuthResult>> RefreshTokensAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return InvalidRefreshToken();
        }

        RefreshToken? storedRefreshToken = await refreshTokenStore.GetByTokenAsync(
            refreshToken,
            cancellationToken);

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;

        if (storedRefreshToken is null || !storedRefreshToken.IsActive(now))
        {
            return InvalidRefreshToken();
        }

        UserDto? user = await userRepository.GetByIdAsync(storedRefreshToken.UserId);

        if (user is null)
        {
            return InvalidRefreshToken();
        }

        (string accessToken, DateTime accessTokenExpiresAt) =
            GenerateAccessToken(user.Id, user.Username, now);

        string newRefreshToken = GenerateRefreshToken();
        DateTime refreshTokenExpiresAt = GetRefreshTokenExpiration(now);
        RefreshToken replacement = new(newRefreshToken, user.Id, refreshTokenExpiresAt, now);

        storedRefreshToken.Revoke(now);

        await refreshTokenStore.AddAsync(replacement, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            accessToken,
            newRefreshToken,
            accessTokenExpiresAt,
            refreshTokenExpiresAt,
            user.Id,
            user.Username);
    }

    private (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(
        string userId,
        string username,
        DateTime issuedAt)
    {
        DateTime expiresAt = issuedAt.AddMinutes(_options.AccessTokenMinutes);

        SecurityTokenDescriptor descriptor = new()
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = issuedAt,
            NotBefore = issuedAt,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
                SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = userId,
                [JwtRegisteredClaimNames.UniqueName] = username,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString("N")
            }
        };

        JsonWebTokenHandler handler = new()
        {
            SetDefaultTimesOnTokenCreation = false
        };

        return (handler.CreateToken(descriptor), expiresAt);
    }

    private static string GenerateRefreshToken() =>
        Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));

    private DateTime GetRefreshTokenExpiration(DateTime now) =>
        UtcDateTimes.NormalizeToMinute(now.AddDays(_options.RefreshTokenDays));

    private static DomainError InvalidRefreshToken() =>
        new(AuthErrors.InvalidRefreshToken, DomainErrorKind.Unauthorized);
}
