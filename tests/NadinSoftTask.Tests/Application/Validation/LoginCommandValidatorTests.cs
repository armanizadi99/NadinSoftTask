using FluentValidation.TestHelper;
using NadinSoftTask.Application.Commands.Login;
using NadinSoftTask.Application.Common;
using NadinSoftTask.Tests.Application.TestData;
using Xunit;

namespace NadinSoftTask.Tests.Application.Validation;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        TestValidationResult<LoginCommand> result = _validator.TestValidate(CreateCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingUsername_HasUsernameError(string? username)
    {
        TestValidationResult<LoginCommand> result =
            _validator.TestValidate(CreateCommand(username: username!));

        result.ShouldHaveValidationErrorFor(command => command.Username)
            .WithErrorMessage("Username cannot be empty or whitespace.");
    }

    [Fact]
    public void Validate_WithUsernameShorterThanRegistrationLimit_HasNoErrors()
    {
        string username = new('a', UserConstraints.MinUsernameLength - 1);

        TestValidationResult<LoginCommand> result =
            _validator.TestValidate(CreateCommand(username: username));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithUsernameLongerThanRegistrationLimit_HasNoErrors()
    {
        string username = new('a', UserConstraints.MaxUsernameLength + 1);

        TestValidationResult<LoginCommand> result =
            _validator.TestValidate(CreateCommand(username: username));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("user name")]
    [InlineData("user-name")]
    [InlineData("user.name")]
    [InlineData("user@name")]
    public void Validate_WithUsernameCharactersRejectedAtRegistration_HasNoErrors(string username)
    {
        TestValidationResult<LoginCommand> result =
            _validator.TestValidate(CreateCommand(username: username));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingPassword_HasPasswordError(string? password)
    {
        TestValidationResult<LoginCommand> result =
            _validator.TestValidate(CreateCommand(password: password!));

        result.ShouldHaveValidationErrorFor(command => command.Password)
            .WithErrorMessage("Password cannot be empty or whitespace.");
    }

    [Fact]
    public void Validate_WithPasswordShorterThanRegistrationLimit_HasNoErrors()
    {
        string password = new('a', UserConstraints.MinPasswordLength - 1);

        TestValidationResult<LoginCommand> result =
            _validator.TestValidate(CreateCommand(password: password));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithPasswordLongerThanRegistrationLimit_HasNoErrors()
    {
        string password = new('a', UserConstraints.MaxPasswordLength + 1);

        TestValidationResult<LoginCommand> result =
            _validator.TestValidate(CreateCommand(password: password));

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static LoginCommand CreateCommand(
        string username = AuthTestData.Username,
        string password = AuthTestData.Password) =>
        new(username, password);
}
