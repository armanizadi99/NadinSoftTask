using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Application.Common.Security;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Infrastructure.Identity;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Infrastructure.Identity;

public class TokenServiceTests
{
    private const string UserId = "user-123";
    private const string Username = "arman";

    private static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTime UtcNow = Now.UtcDateTime;
    private static readonly DateTime AccessTokenExpiresAt = new(2026, 1, 15, 12, 15, 0, DateTimeKind.Utc);
    private static readonly DateTime RefreshTokenExpiresAt = new(2026, 1, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task GenerateTokensAsync_ReturnsSignedAccessTokenAndPersistsRefreshToken()
    {
        TestContext context = new();
        RefreshToken? added = null;

        context.Store
            .Setup(store => store.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((refreshToken, _) => added = refreshToken)
            .Returns(Task.CompletedTask);
        context.Store
            .Setup(store => store.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        Result<AuthResult> result = await context.CreateService().GenerateTokensAsync(
            UserId,
            Username,
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);

        AuthResult authResult = result.Value!;

        JsonWebToken jwt = new(authResult.AccessToken);

        jwt.Issuer.ShouldBe("test-issuer");
        jwt.Audiences.ShouldContain("test-audience");
        jwt.Subject.ShouldBe(UserId);
        jwt.Claims.Single(claim => claim.Type == "unique_name").Value.ShouldBe(Username);
        jwt.ValidTo.ShouldBe(AccessTokenExpiresAt, TimeSpan.FromSeconds(1));

        authResult.AccessTokenExpiresAt.ShouldBe(AccessTokenExpiresAt);
        authResult.RefreshToken.Length.ShouldBe(43);
        authResult.RefreshTokenExpiresAt.ShouldBe(RefreshTokenExpiresAt);
        authResult.UserId.ShouldBe(UserId);
        authResult.Username.ShouldBe(Username);

        added.ShouldNotBeNull();
        added!.UserId.ShouldBe(UserId);
        added.ExpiresAt.ShouldBe(RefreshTokenExpiresAt);
        added.CreatedAt.ShouldBe(UtcNow);
        added.IsRevoked.ShouldBeFalse();
        added.TokenHash.ShouldBe(RefreshToken.HashToken(authResult.RefreshToken));

        context.Store.Verify(
            store => store.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokensAsync_WithActiveToken_RevokesOldTokenAndIssuesReplacement()
    {
        TestContext context = new();
        RefreshToken storedRefreshToken = CreateToken(
            token: "stored-token",
            expiresAt: UtcNow.AddMinutes(5));
        List<RefreshToken> added = [];

        context.Store
            .Setup(store => store.GetByTokenAsync(
                "stored-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedRefreshToken);
        context.Store
            .Setup(store => store.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((refreshToken, _) => added.Add(refreshToken))
            .Returns(Task.CompletedTask);
        context.Store
            .Setup(store => store.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        context.UserRepository
            .Setup(repository => repository.GetByIdAsync(
                UserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserDto(UserId, Username));

        Result<AuthResult> result = await context.CreateService().RefreshTokensAsync(
            "stored-token",
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);

        AuthResult authResult = result.Value!;

        storedRefreshToken.IsRevoked.ShouldBeTrue();
        storedRefreshToken.RevokedAt.ShouldBe(UtcNow);

        added.Count.ShouldBe(1);

        RefreshToken replacement = added[0];

        replacement.UserId.ShouldBe(UserId);
        replacement.ExpiresAt.ShouldBe(RefreshTokenExpiresAt);
        replacement.CreatedAt.ShouldBe(UtcNow);
        replacement.IsRevoked.ShouldBeFalse();
        replacement.TokenHash.ShouldBe(RefreshToken.HashToken(authResult.RefreshToken));

        authResult.RefreshToken.ShouldNotBe("stored-token");
        authResult.RefreshToken.Length.ShouldBe(43);
        authResult.AccessTokenExpiresAt.ShouldBe(AccessTokenExpiresAt);
        authResult.RefreshTokenExpiresAt.ShouldBe(RefreshTokenExpiresAt);
        authResult.UserId.ShouldBe(UserId);
        authResult.Username.ShouldBe(Username);

        context.UserRepository.Verify(
            repository => repository.GetByIdAsync(
                UserId,
                It.IsAny<CancellationToken>()),
            Times.Once);
        context.Store.Verify(
            store => store.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokensAsync_WhenTokenDoesNotExist_ReturnsNotAuthorized()
    {
        TestContext context = new();

        context.Store
            .Setup(store => store.GetByTokenAsync(
                "missing-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        Result<AuthResult> result = await context.CreateService().RefreshTokensAsync(
            "missing-token",
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);

        context.Store.Verify(
            store => store.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        context.Store.Verify(
            store => store.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        context.UserRepository.Verify(
            repository => repository.GetByIdAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokensAsync_WhenTokenIsExpired_ReturnsNotAuthorized()
    {
        TestContext context = new();

        context.Store
            .Setup(store => store.GetByTokenAsync(
                "expired-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateToken(
                token: "expired-token",
                expiresAt: UtcNow.AddMinutes(-1)));

        Result<AuthResult> result = await context.CreateService().RefreshTokensAsync(
            "expired-token",
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);

        context.Store.Verify(
            store => store.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        context.Store.Verify(
            store => store.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokensAsync_WhenTokenIsRevoked_ReturnsNotAuthorized()
    {
        TestContext context = new();
        RefreshToken storedRefreshToken = CreateToken(
            token: "revoked-token",
            expiresAt: UtcNow.AddMinutes(5));

        storedRefreshToken.Revoke(UtcNow);

        context.Store
            .Setup(store => store.GetByTokenAsync(
                "revoked-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedRefreshToken);

        Result<AuthResult> result = await context.CreateService().RefreshTokensAsync(
            "revoked-token",
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);

        context.Store.Verify(
            store => store.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        context.Store.Verify(
            store => store.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokensAsync_WhenUserNoLongerExists_ReturnsNotAuthorized()
    {
        TestContext context = new();

        context.Store
            .Setup(store => store.GetByTokenAsync(
                "stored-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateToken(
                token: "stored-token",
                expiresAt: UtcNow.AddMinutes(5)));
        context.UserRepository
            .Setup(repository => repository.GetByIdAsync(
                UserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        Result<AuthResult> result = await context.CreateService().RefreshTokensAsync(
            "stored-token",
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);

        context.Store.Verify(
            store => store.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        context.Store.Verify(
            store => store.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RefreshTokensAsync_WithMissingToken_ReturnsNotAuthorized(string refreshToken)
    {
        TestContext context = new();

        Result<AuthResult> result = await context.CreateService().RefreshTokensAsync(
            refreshToken,
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);

        context.Store.Verify(
            store => store.GetByTokenAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static RefreshToken CreateToken(string token, DateTime expiresAt) =>
        new(token, UserId, expiresAt, UtcNow);

    private sealed class TestContext
    {
        public Mock<IRefreshTokenStore> Store { get; } = new(MockBehavior.Strict);

        public Mock<IUserRepository> UserRepository { get; } = new(MockBehavior.Strict);

        public TokenService CreateService() =>
            new(
                Store.Object,
                UserRepository.Object,
                Options.Create(new JwtOptions
                {
                    Issuer = "test-issuer",
                    Audience = "test-audience",
                    SigningKey = new string('k', 64),
                    AccessTokenMinutes = 15,
                    RefreshTokenDays = 7
                }),
                new FixedTimeProvider(Now));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
