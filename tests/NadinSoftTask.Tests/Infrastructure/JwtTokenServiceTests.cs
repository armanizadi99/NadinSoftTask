using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using NadinSoftTask.Application.Common.Security;
using NadinSoftTask.Infrastructure.Security;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Infrastructure;

public class JwtTokenServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void GenerateAccessToken_ContainsConfiguredClaims()
    {
        JwtTokenService service = CreateService();

        (string token, DateTime expiresAt) = service.GenerateAccessToken("user-id", "arman");

        JsonWebToken jwt = new(token);

        jwt.Issuer.ShouldBe("test-issuer");
        jwt.Audiences.ShouldContain("test-audience");
        jwt.Subject.ShouldBe("user-id");
        jwt.Claims.Single(claim => claim.Type == "unique_name").Value.ShouldBe("arman");
        expiresAt.ShouldBe(Now.UtcDateTime.AddMinutes(15));
        jwt.ValidTo.ShouldBe(expiresAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsDistinctBase64UrlValues()
    {
        JwtTokenService service = CreateService();

        string first = service.GenerateRefreshToken();
        string second = service.GenerateRefreshToken();

        first.Length.ShouldBe(43);
        second.Length.ShouldBe(43);
        first.ShouldNotBe(second);
    }

    [Fact]
    public void GetRefreshTokenExpiration_TruncatesToMinute()
    {
        JwtTokenService service = CreateService();

        DateTime expiration = service.GetRefreshTokenExpiration();

        expiration.ShouldBe(new DateTime(2026, 1, 22, 12, 0, 0, DateTimeKind.Utc));
        expiration.Second.ShouldBe(0);
        expiration.Millisecond.ShouldBe(0);
    }

    private static JwtTokenService CreateService() =>
        new(
            Options.Create(new JwtOptions
            {
                Issuer = "test-issuer",
                Audience = "test-audience",
                SigningKey = new string('k', 64),
                AccessTokenMinutes = 15,
                RefreshTokenDays = 7
            }),
            new FixedTimeProvider(Now));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
