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
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        IsRevoked = false;
    }

    public int Id { get; private set; }

    public string Token { get; private set; } = null!;

    public string UserId { get; private set; } = null!;

    public DateTime ExpiresAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public bool IsRevoked { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public void Revoke()
    {
        if (IsRevoked)
        {
            return;
        }

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsActive(DateTime now) =>
        !IsRevoked && ExpiresAt > now;
}
