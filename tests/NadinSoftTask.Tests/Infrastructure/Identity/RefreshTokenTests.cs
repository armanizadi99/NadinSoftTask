using NadinSoftTask.Infrastructure.Identity;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Infrastructure.Identity;

public class RefreshTokenTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 12, 0, 30, 123, DateTimeKind.Utc);

    [Fact]
    public void IsActive_WithFutureExpirationAndNotRevoked_ReturnsTrue()
    {
        RefreshToken refreshToken = CreateToken(expiresAt: Now.AddMinutes(5));

        refreshToken.IsActive(Now).ShouldBeTrue();
    }

    [Fact]
    public void IsActive_WithPastExpiration_ReturnsFalse()
    {
        RefreshToken refreshToken = CreateToken(expiresAt: Now.AddMinutes(-1));

        refreshToken.IsActive(Now).ShouldBeFalse();
    }

    [Fact]
    public void IsActive_WithRevokedToken_ReturnsFalse()
    {
        RefreshToken refreshToken = CreateToken(expiresAt: Now.AddMinutes(5));
        refreshToken.Revoke(Now);

        refreshToken.IsActive(Now).ShouldBeFalse();
    }

    [Fact]
    public void Revoke_SetsRevokedAt()
    {
        RefreshToken refreshToken = CreateToken(expiresAt: Now.AddMinutes(5));

        refreshToken.Revoke(Now);

        refreshToken.IsRevoked.ShouldBeTrue();
        refreshToken.RevokedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_ComputesSha256TokenHash()
    {
        RefreshToken refreshToken = CreateToken();

        refreshToken.TokenHash.Length.ShouldBe(32);
        refreshToken.TokenHash.ShouldBe(RefreshToken.HashToken("token"));
        refreshToken.TokenHash.ShouldNotBe(RefreshToken.HashToken("other-token"));
    }

    [Fact]
    public void Constructor_TruncatesTimestampsToMinute()
    {
        RefreshToken refreshToken = new("token", "user-1", Now.AddDays(7), Now);

        refreshToken.ExpiresAt.ShouldBe(new DateTime(2026, 1, 22, 12, 0, 0, DateTimeKind.Utc));
        refreshToken.CreatedAt.ShouldBe(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Revoke_TruncatesRevokedAtToMinute()
    {
        RefreshToken refreshToken = CreateToken(expiresAt: Now.AddMinutes(5));

        refreshToken.Revoke(Now);

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
            new RefreshToken(token!, "user-1", Now.AddMinutes(5), Now));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithMissingUserId_Throws(string? userId)
    {
        Should.Throw<ArgumentException>(() =>
            new RefreshToken("token", userId!, Now.AddMinutes(5), Now));
    }

    private static RefreshToken CreateToken(DateTime? expiresAt = null) =>
        new("token", "user-1", expiresAt ?? Now.AddMinutes(5), Now);
}
