using NadinSoftTask.Application.Commands.RefreshTokens;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Tests.Application.TestData;
using NadinSoftTask.Tests.Application.TestSupport;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Application.Commands;

public class RefreshTokenCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRefreshToken_ReturnsIssuedTokens()
    {
        RefreshTokenCommand command = new(AuthTestData.RefreshToken);
        AuthTestContext context = new();
        AuthResult authResult = AuthTestData.CreateAuthResult(AuthTestData.NewRefreshToken);

        context.GivenRefreshTokens(command.RefreshToken, authResult);

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);
        result.Value.ShouldBe(authResult);

        context.VerifyRefreshTokensOnce(command.RefreshToken);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenIsRejected_PropagatesUnauthorized()
    {
        RefreshTokenCommand command = new(AuthTestData.RefreshToken);
        AuthTestContext context = new();
        DomainError error = new("Invalid refresh token.", DomainErrorKind.Unauthorized);

        context.GivenRefreshTokens(
            command.RefreshToken,
            Result<AuthResult>.Failure(error));

        Result<AuthResult> result = await CreateHandler(context).Handle(
            command,
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.ShouldBe(error);

        context.VerifyRefreshTokensOnce(command.RefreshToken);
    }

    private static RefreshTokenCommandHandler CreateHandler(AuthTestContext context) =>
        new(context.TokenService.Object);
}
