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
    public async Task Handle_WithValidCommand_CreatesUserAndReturnsIssuedTokens()
    {
        RegisterUserCommand command = new(AuthTestData.Username, AuthTestData.Password);
        AuthTestContext context = new();
        UserDto user = AuthTestData.User;
        AuthResult authResult = AuthTestData.CreateAuthResult();

        context.GivenCreateSucceeds(command.Username, command.Password, user);
        context.GivenGenerateTokens(user.Id, user.Username, authResult);

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);
        result.Value.ShouldBe(authResult);

        context.VerifyCreatedOnce(command.Username, command.Password);
        context.VerifyGenerateTokensOnce(user.Id, user.Username);
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
        context.VerifyGenerateTokensNotCalled();
    }

    [Fact]
    public async Task Handle_WhenUserCreationFails_PropagatesErrorAndDoesNotIssueTokens()
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
        context.VerifyGenerateTokensNotCalled();
    }

    private static RegisterUserCommandHandler CreateHandler(AuthTestContext context) =>
        new(
            context.UserRepository.Object,
            context.TokenService.Object);
}
