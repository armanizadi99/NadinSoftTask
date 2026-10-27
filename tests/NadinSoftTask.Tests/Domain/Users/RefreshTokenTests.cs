using NadinSoftTask.Domain.Users;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Domain.Users;

public class RefreshTokenTests
{
    [Fact]
    public void IsActive_WithFutureExpirationAndNotRevoked_ReturnsTrue()
    {
        RefreshToken refreshToken = new("token", "user-1", DateTime.UtcNow.AddMinutes(5));

        refreshToken.IsActive(DateTime.UtcNow).ShouldBeTrue();
    }

    [Fact]
    public void IsActive_WithPastExpiration_ReturnsFalse()
    {
        RefreshToken refreshToken = new("token", "user-1", DateTime.UtcNow.AddMinutes(-1));

        refreshToken.IsActive(DateTime.UtcNow).ShouldBeFalse();
    }

    [Fact]
    public void IsActive_WithRevokedToken_ReturnsFalse()
    {
        RefreshToken refreshToken = new("token", "user-1", DateTime.UtcNow.AddMinutes(5));
        refreshToken.Revoke();

        refreshToken.IsActive(DateTime.UtcNow).ShouldBeFalse();
    }

    [Fact]
    public void Revoke_SetsRevokedAt()
    {
        RefreshToken refreshToken = new("token", "user-1", DateTime.UtcNow.AddMinutes(5));

        refreshToken.Revoke();

        refreshToken.IsRevoked.ShouldBeTrue();
        refreshToken.RevokedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_ComputesSha256TokenHash()
    {
        RefreshToken refreshToken = new("token", "user-1", DateTime.UtcNow.AddMinutes(5));

        refreshToken.TokenHash.Length.ShouldBe(32);
        refreshToken.TokenHash.ShouldBe(RefreshToken.HashToken("token"));
        refreshToken.TokenHash.ShouldNotBe(RefreshToken.HashToken("other-token"));
    }

    [Fact]
    public void Constructor_TruncatesTimestampsToMinute()
    {
        DateTime expiresAt = new(2026, 1, 15, 12, 30, 45, 123, DateTimeKind.Utc);

        RefreshToken refreshToken = new("token", "user-1", expiresAt);

        refreshToken.ExpiresAt.ShouldBe(new DateTime(2026, 1, 15, 12, 30, 0, DateTimeKind.Utc));
        refreshToken.CreatedAt.Second.ShouldBe(0);
        refreshToken.CreatedAt.Millisecond.ShouldBe(0);
    }

    [Fact]
    public void Revoke_TruncatesRevokedAtToMinute()
    {
        RefreshToken refreshToken = new("token", "user-1", DateTime.UtcNow.AddMinutes(5));

        refreshToken.Revoke();

        refreshToken.RevokedAt!.Value.Second.ShouldBe(0);
        refreshToken.RevokedAt.Value.Millisecond.ShouldBe(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithMissingToken_Throws(string? token)
    {
        Should.Throw<ArgumentException>(() =>
            new RefreshToken(token!, "user-1", DateTime.UtcNow.AddMinutes(5)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithMissingUserId_Throws(string? userId)
    {
        Should.Throw<ArgumentException>(() =>
            new RefreshToken("token", userId!, DateTime.UtcNow.AddMinutes(5)));
    }
}
