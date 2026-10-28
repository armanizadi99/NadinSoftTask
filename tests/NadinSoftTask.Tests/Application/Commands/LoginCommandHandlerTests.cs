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
    public async Task Handle_WithValidCredentials_ReturnsIssuedTokens()
    {
        LoginCommand command = new(AuthTestData.Username, AuthTestData.Password);
        AuthTestContext context = new();
        UserDto user = AuthTestData.User;
        AuthResult authResult = AuthTestData.CreateAuthResult();

        context.GivenUser(command.Username, user);
        context.GivenPasswordValidity(user.Id, command.Password, isValid: true);
        context.GivenGenerateTokens(user.Id, user.Username, authResult);

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);
        result.Value.ShouldBe(authResult);

        context.VerifyUserLookupOnce(command.Username);
        context.VerifyPasswordCheckedOnce(user.Id, command.Password);
        context.VerifyGenerateTokensOnce(user.Id, user.Username);
    }

    [Fact]
    public async Task Handle_WhenUsernameDoesNotExist_ReturnsUnauthorizedAndDoesNotIssueTokens()
    {
        LoginCommand command = new(AuthTestData.Username, AuthTestData.Password);
        AuthTestContext context = new();

        context.GivenMissingUser(command.Username);

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.Unauthorized);

        context.VerifyUserLookupOnce(command.Username);
        context.VerifyPasswordNotChecked();
        context.VerifyGenerateTokensNotCalled();
    }

    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_ReturnsUnauthorizedAndDoesNotIssueTokens()
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
        result.Error.Kind.ShouldBe(DomainErrorKind.Unauthorized);

        context.VerifyUserLookupOnce(command.Username);
        context.VerifyPasswordCheckedOnce(user.Id, command.Password);
        context.VerifyGenerateTokensNotCalled();
    }

    private static LoginCommandHandler CreateHandler(AuthTestContext context) =>
        new(
            context.UserRepository.Object,
            context.TokenService.Object);
}
