using NadinSoftTask.Application.Commands.RegisterUser;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Tests.Application.TestData;
using NadinSoftTask.Tests.Application.TestSupport;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Application.Commands;

public class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_CreatesUserIssuesTokensPersistsRefreshTokenAndSaves()
    {
        RegisterUserCommand command = new(AuthTestData.Username, AuthTestData.Password);
        AuthTestContext context = new();
        UserDto user = AuthTestData.User;

        context.GivenCreateSucceeds(command.Username, command.Password, user);
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

        context.VerifyCreatedOnce(command.Username, command.Password);
        context.VerifyAccessTokenGeneratedOnce(user.Id, user.Username);
        context.VerifyRefreshTokenGeneratedOnce();
        context.VerifyRefreshTokenExpirationRequestedOnce();
        context.VerifyRefreshTokenAddedOnce();
        context.VerifySavedOnce();
    }

    [Fact]
    public async Task Handle_WhenUsernameAlreadyExists_ReturnsConflictAndDoesNotIssueTokens()
    {
        RegisterUserCommand command = new(AuthTestData.Username, AuthTestData.Password);
        AuthTestContext context = new();
        DomainError conflict = new(
            "A user with the same username already exists.",
            DomainErrorKind.Conflict);

        context.GivenCreateFails(command.Username, command.Password, conflict);

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.Conflict);

        context.VerifyCreatedOnce(command.Username, command.Password);
        context.VerifyAccessTokenNotGenerated();
        context.VerifyRefreshTokenNotGenerated();
        context.VerifyRefreshTokenExpirationNotRequested();
        context.VerifyNotAdded();
        context.VerifyNotSaved();
    }

    [Fact]
    public async Task Handle_WhenUserCreationFails_PropagatesErrorAndDoesNotPersist()
    {
        RegisterUserCommand command = new(AuthTestData.Username, AuthTestData.Password);
        AuthTestContext context = new();
        DomainError validationError = new(
            "Password is too weak.",
            DomainErrorKind.Validation);

        context.GivenCreateFails(command.Username, command.Password, validationError);

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.ShouldBe(validationError);

        context.VerifyCreatedOnce(command.Username, command.Password);
        context.VerifyAccessTokenNotGenerated();
        context.VerifyRefreshTokenNotGenerated();
        context.VerifyRefreshTokenExpirationNotRequested();
        context.VerifyNotAdded();
        context.VerifyNotSaved();
    }

    private static RegisterUserCommandHandler CreateHandler(AuthTestContext context) =>
        new(
            context.UserRepository.Object,
            context.TokenService.Object,
            context.RefreshTokenRepository.Object,
            context.UnitOfWork.Object);
}
