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
