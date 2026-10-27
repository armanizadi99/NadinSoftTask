using NadinSoftTask.Application.Commands.RefreshTokens;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Users;
using NadinSoftTask.Tests.Application.TestData;
using NadinSoftTask.Tests.Application.TestSupport;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Application.Commands;

public class RefreshTokenCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithActiveToken_RotatesTokenIssuesNewTokensAndSaves()
    {
        UserDto user = AuthTestData.User;
        RefreshToken storedRefreshToken = AuthTestData.CreateRefreshToken(
            expiresAt: DateTime.UtcNow.AddMinutes(5));
        RefreshTokenCommand command = new(storedRefreshToken.Token);
        AuthTestContext context = new();
        DateTime newRefreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(10);

        context.GivenStoredRefreshToken(command.RefreshToken, storedRefreshToken);
        context.GivenUserById(user.Id, user);
        context.GivenAccessToken(
            user.Id,
            user.Username,
            AuthTestData.AccessToken,
            AuthTestData.AccessTokenExpiresAt);
        context.GivenRefreshToken(AuthTestData.NewRefreshToken);
        context.GivenRefreshTokenExpiration(newRefreshTokenExpiresAt);
        context.GivenAddSucceeds(refreshToken =>
            refreshToken.Token == AuthTestData.NewRefreshToken &&
            refreshToken.UserId == user.Id &&
            refreshToken.ExpiresAt == newRefreshTokenExpiresAt &&
            !refreshToken.IsRevoked);

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);
        result.Value.ShouldBe(new AuthResult(
            AuthTestData.AccessToken,
            AuthTestData.NewRefreshToken,
            AuthTestData.AccessTokenExpiresAt,
            newRefreshTokenExpiresAt,
            user.Id,
            user.Username));

        storedRefreshToken.IsRevoked.ShouldBeTrue();
        storedRefreshToken.RevokedAt.ShouldNotBeNull();

        context.VerifyRefreshTokenLookupOnce(command.RefreshToken);
        context.VerifyUserByIdLookupOnce(user.Id);
        context.VerifyAccessTokenGeneratedOnce(user.Id, user.Username);
        context.VerifyRefreshTokenGeneratedOnce();
        context.VerifyRefreshTokenExpirationRequestedOnce();
        context.VerifyRefreshTokenAddedOnce();
        context.VerifySavedOnce();
    }

    [Fact]
    public async Task Handle_WhenTokenDoesNotExist_ReturnsNotAuthorizedAndDoesNotIssueTokens()
    {
        RefreshTokenCommand command = new(AuthTestData.RefreshToken);
        AuthTestContext context = new();

        context.GivenMissingRefreshToken(command.RefreshToken);

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);

        context.VerifyRefreshTokenLookupOnce(command.RefreshToken);
        context.VerifyUserByIdNotLookedUp();
        context.VerifyAccessTokenNotGenerated();
        context.VerifyRefreshTokenNotGenerated();
        context.VerifyRefreshTokenExpirationNotRequested();
        context.VerifyNotAdded();
        context.VerifyNotSaved();
    }

    [Fact]
    public async Task Handle_WhenTokenIsExpired_ReturnsNotAuthorizedAndDoesNotIssueTokens()
    {
        RefreshToken storedRefreshToken = AuthTestData.CreateRefreshToken(
            expiresAt: DateTime.UtcNow.AddMinutes(-1));
        RefreshTokenCommand command = new(storedRefreshToken.Token);
        AuthTestContext context = new();

        context.GivenStoredRefreshToken(command.RefreshToken, storedRefreshToken);

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);

        context.VerifyRefreshTokenLookupOnce(command.RefreshToken);
        context.VerifyUserByIdNotLookedUp();
        context.VerifyAccessTokenNotGenerated();
        context.VerifyRefreshTokenNotGenerated();
        context.VerifyRefreshTokenExpirationNotRequested();
        context.VerifyNotAdded();
        context.VerifyNotSaved();
    }

    [Fact]
    public async Task Handle_WhenTokenIsRevoked_ReturnsNotAuthorizedAndDoesNotIssueTokens()
    {
        RefreshToken storedRefreshToken = AuthTestData.CreateRefreshToken();
        storedRefreshToken.Revoke();
        RefreshTokenCommand command = new(storedRefreshToken.Token);
        AuthTestContext context = new();

        context.GivenStoredRefreshToken(command.RefreshToken, storedRefreshToken);

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);

        context.VerifyRefreshTokenLookupOnce(command.RefreshToken);
        context.VerifyUserByIdNotLookedUp();
        context.VerifyAccessTokenNotGenerated();
        context.VerifyRefreshTokenNotGenerated();
        context.VerifyRefreshTokenExpirationNotRequested();
        context.VerifyNotAdded();
        context.VerifyNotSaved();
    }

    [Fact]
    public async Task Handle_WhenUserNoLongerExists_ReturnsNotAuthorizedAndDoesNotIssueTokens()
    {
        UserDto user = AuthTestData.User;
        RefreshToken storedRefreshToken = AuthTestData.CreateRefreshToken(
            expiresAt: DateTime.UtcNow.AddMinutes(5));
        RefreshTokenCommand command = new(storedRefreshToken.Token);
        AuthTestContext context = new();

        context.GivenStoredRefreshToken(command.RefreshToken, storedRefreshToken);
        context.GivenMissingUserById(user.Id);

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);

        context.VerifyRefreshTokenLookupOnce(command.RefreshToken);
        context.VerifyUserByIdLookupOnce(user.Id);
        context.VerifyAccessTokenNotGenerated();
        context.VerifyRefreshTokenNotGenerated();
        context.VerifyRefreshTokenExpirationNotRequested();
        context.VerifyNotAdded();
        context.VerifyNotSaved();
    }

    private static RefreshTokenCommandHandler CreateHandler(AuthTestContext context) =>
        new(
            context.RefreshTokenRepository.Object,
            context.UserRepository.Object,
            context.TokenService.Object,
            context.UnitOfWork.Object);
}
