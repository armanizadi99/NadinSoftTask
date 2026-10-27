using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Security;

namespace NadinSoftTask.Infrastructure.Security;

public sealed class JwtTokenService(
    IOptions<JwtOptions> options,
    TimeProvider timeProvider) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(
        string userId,
        string username)
    {
        DateTime issuedAt = timeProvider.GetUtcNow().UtcDateTime;
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

    public string GenerateRefreshToken() =>
        Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));

    public DateTime GetRefreshTokenExpiration() =>
        TruncateToMinute(timeProvider.GetUtcNow().UtcDateTime.AddDays(_options.RefreshTokenDays));

    private static DateTime TruncateToMinute(DateTime value) =>
        new(
            value.Ticks - (value.Ticks % TimeSpan.TicksPerMinute),
            DateTimeKind.Utc);
}
