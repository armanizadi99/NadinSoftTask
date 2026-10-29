using System.Security.Cryptography;
using System.Text;
using NadinSoftTask.Infrastructure.Common;

namespace NadinSoftTask.Infrastructure.Identity;

public sealed class RefreshToken
{
    private RefreshToken()
    {
    }

    public RefreshToken(string token, string userId, DateTime expiresAt, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Refresh token cannot be empty or whitespace.", nameof(token));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User id cannot be empty or whitespace.", nameof(userId));
        }

        TokenHash = HashToken(token);
        UserId = userId;
        ExpiresAt = NormalizeUtc(expiresAt);
        CreatedAt = NormalizeUtc(createdAt);
    }

    public int Id { get; private set; }

    // Only the hash is persisted; the raw token is returned to the client and never stored.
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

    public void Revoke(DateTime revokedAt)
    {
        if (RevokedAt.HasValue)
        {
            return;
        }

        RevokedAt = NormalizeUtc(revokedAt);
    }

    public bool IsActive(DateTime now) =>
        !IsRevoked && ExpiresAt > now;

    private static DateTime NormalizeUtc(DateTime value) =>
        UtcDateTimes.NormalizeToMinute(value);
}
