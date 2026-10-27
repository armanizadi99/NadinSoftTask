using System.Security.Cryptography;
using System.Text;

namespace NadinSoftTask.Domain.Users;

public sealed class RefreshToken
{
    private RefreshToken()
    {
    }

    public RefreshToken(string token, string userId, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Refresh token cannot be empty or whitespace.", nameof(token));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User id cannot be empty or whitespace.", nameof(userId));
        }

        Token = token;
        TokenHash = HashToken(token);
        UserId = userId;
        ExpiresAt = NormalizeUtc(expiresAt);
        CreatedAt = NormalizeUtc(DateTime.UtcNow);
    }

    public int Id { get; private set; }

    // The raw token is returned to the client and never persisted; only its hash is stored.
    public string Token { get; private set; } = null!;

    public byte[] TokenHash { get; private set; } = null!;

    public string UserId { get; private set; } = null!;

    public DateTime ExpiresAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;

    public DateTime? RevokedAt { get; private set; }

    public static byte[] HashToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Refresh token cannot be empty or whitespace.", nameof(token));
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(token));
    }

    public void Revoke()
    {
        if (RevokedAt.HasValue)
        {
            return;
        }

        RevokedAt = NormalizeUtc(DateTime.UtcNow);
    }

    public bool IsActive(DateTime now) =>
        !IsRevoked && ExpiresAt > now;

    private static DateTime NormalizeUtc(DateTime value)
    {
        DateTime utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return new DateTime(
            utc.Ticks - (utc.Ticks % TimeSpan.TicksPerMinute),
            DateTimeKind.Utc);
    }
}
