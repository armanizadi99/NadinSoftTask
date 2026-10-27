using FluentValidation.TestHelper;
using NadinSoftTask.Application.Commands.RefreshTokens;
using NadinSoftTask.Tests.Application.TestData;
using Xunit;

namespace NadinSoftTask.Tests.Application.Validation;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        TestValidationResult<RefreshTokenCommand> result =
            _validator.TestValidate(new RefreshTokenCommand(AuthTestData.RefreshToken));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingRefreshToken_HasRefreshTokenError(string? refreshToken)
    {
        TestValidationResult<RefreshTokenCommand> result =
            _validator.TestValidate(new RefreshTokenCommand(refreshToken!));

        result.ShouldHaveValidationErrorFor(command => command.RefreshToken)
            .WithErrorMessage("Refresh token cannot be empty or whitespace.");
    }
}
