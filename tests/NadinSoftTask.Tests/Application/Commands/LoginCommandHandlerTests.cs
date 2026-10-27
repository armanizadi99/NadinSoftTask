using NadinSoftTask.Application.Commands.Login;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Tests.Application.TestData;
using NadinSoftTask.Tests.Application.TestSupport;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Application.Commands;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCredentials_IssuesTokensPersistsRefreshTokenAndSaves()
    {
        LoginCommand command = new(AuthTestData.Username, AuthTestData.Password);
        AuthTestContext context = new();
        UserDto user = AuthTestData.User;

        context.GivenUser(command.Username, user);
        context.GivenPasswordValidity(user.Id, command.Password, isValid: true);
        context.GivenAccessToken(
            user.Id,
            user.Username,
            AuthTestData.AccessToken,
            AuthTestData.AccessTokenExpiresAt);
        context.GivenRefreshToken(AuthTestData.RefreshToken);
        context.GivenRefreshTokenExpiration(AuthTestData.RefreshTokenExpiresAt);
        context.GivenAddSucceeds(refreshToken =>
            refreshToken.Token == AuthTestData.RefreshToken &&
            refreshToken.UserId == user.Id &&
            refreshToken.ExpiresAt == AuthTestData.RefreshTokenExpiresAt &&
            !refreshToken.IsRevoked);

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);
        result.Value.ShouldBe(new AuthResult(
            AuthTestData.AccessToken,
            AuthTestData.RefreshToken,
            AuthTestData.AccessTokenExpiresAt,
            AuthTestData.RefreshTokenExpiresAt,
            user.Id,
            user.Username));

        context.VerifyUserLookupOnce(command.Username);
        context.VerifyPasswordCheckedOnce(user.Id, command.Password);
        context.VerifyAccessTokenGeneratedOnce(user.Id, user.Username);
        context.VerifyRefreshTokenGeneratedOnce();
        context.VerifyRefreshTokenExpirationRequestedOnce();
        context.VerifyRefreshTokenAddedOnce();
        context.VerifySavedOnce();
    }

    [Fact]
    public async Task Handle_WhenUsernameDoesNotExist_ReturnsNotAuthorizedAndDoesNotIssueTokens()
    {
        LoginCommand command = new(AuthTestData.Username, AuthTestData.Password);
        AuthTestContext context = new();

        context.GivenMissingUser(command.Username);

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);

        context.VerifyUserLookupOnce(command.Username);
        context.VerifyPasswordNotChecked();
        context.VerifyAccessTokenNotGenerated();
        context.VerifyRefreshTokenNotGenerated();
        context.VerifyRefreshTokenExpirationNotRequested();
        context.VerifyNotAdded();
        context.VerifyNotSaved();
    }

    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_ReturnsNotAuthorizedAndDoesNotIssueTokens()
    {
        LoginCommand command = new(AuthTestData.Username, AuthTestData.Password);
        AuthTestContext context = new();
        UserDto user = AuthTestData.User;

        context.GivenUser(command.Username, user);
        context.GivenPasswordValidity(user.Id, command.Password, isValid: false);

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.NotAuthorized);

        context.VerifyUserLookupOnce(command.Username);
        context.VerifyPasswordCheckedOnce(user.Id, command.Password);
        context.VerifyAccessTokenNotGenerated();
        context.VerifyRefreshTokenNotGenerated();
        context.VerifyRefreshTokenExpirationNotRequested();
        context.VerifyNotAdded();
        context.VerifyNotSaved();
    }

    private static LoginCommandHandler CreateHandler(AuthTestContext context) =>
        new(
            context.UserRepository.Object,
            context.TokenService.Object,
            context.RefreshTokenRepository.Object,
            context.UnitOfWork.Object);
}
