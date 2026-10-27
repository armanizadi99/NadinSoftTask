using FluentValidation.TestHelper;
using NadinSoftTask.Application.Commands.RegisterUser;
using NadinSoftTask.Application.Common;
using NadinSoftTask.Tests.Application.TestData;
using Xunit;

namespace NadinSoftTask.Tests.Application.Validation;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        TestValidationResult<RegisterUserCommand> result = _validator.TestValidate(CreateCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingUsername_HasUsernameError(string? username)
    {
        TestValidationResult<RegisterUserCommand> result =
            _validator.TestValidate(CreateCommand(username: username!));

        result.ShouldHaveValidationErrorFor(command => command.Username)
            .WithErrorMessage("Username cannot be empty or whitespace.");
    }

    [Fact]
    public void Validate_WithTooShortUsername_HasUsernameError()
    {
        string username = new('a', UserConstraints.MinUsernameLength - 1);

        TestValidationResult<RegisterUserCommand> result =
            _validator.TestValidate(CreateCommand(username: username));

        result.ShouldHaveValidationErrorFor(command => command.Username)
            .WithErrorMessage(
                $"Username length must be between {UserConstraints.MinUsernameLength} and {UserConstraints.MaxUsernameLength} characters.");
    }

    [Fact]
    public void Validate_WithTooLongUsername_HasUsernameError()
    {
        string username = new('a', UserConstraints.MaxUsernameLength + 1);

        TestValidationResult<RegisterUserCommand> result =
            _validator.TestValidate(CreateCommand(username: username));

        result.ShouldHaveValidationErrorFor(command => command.Username)
            .WithErrorMessage(
                $"Username length must be between {UserConstraints.MinUsernameLength} and {UserConstraints.MaxUsernameLength} characters.");
    }

    [Theory]
    [InlineData("user name")]
    [InlineData("user-name")]
    [InlineData("user.name")]
    [InlineData("user@name")]
    public void Validate_WithInvalidUsernameCharacters_HasUsernameError(string username)
    {
        TestValidationResult<RegisterUserCommand> result =
            _validator.TestValidate(CreateCommand(username: username));

        result.ShouldHaveValidationErrorFor(command => command.Username)
            .WithErrorMessage("Username can only contain letters, numbers, and underscores.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingPassword_HasPasswordError(string? password)
    {
        TestValidationResult<RegisterUserCommand> result =
            _validator.TestValidate(CreateCommand(password: password!));

        result.ShouldHaveValidationErrorFor(command => command.Password)
            .WithErrorMessage("Password cannot be empty or whitespace.");
    }

    [Fact]
    public void Validate_WithTooShortPassword_HasPasswordError()
    {
        string password = new('a', UserConstraints.MinPasswordLength - 1);

        TestValidationResult<RegisterUserCommand> result =
            _validator.TestValidate(CreateCommand(password: password));

        result.ShouldHaveValidationErrorFor(command => command.Password)
            .WithErrorMessage(
                $"Password length must be between {UserConstraints.MinPasswordLength} and {UserConstraints.MaxPasswordLength} characters.");
    }

    [Fact]
    public void Validate_WithTooLongPassword_HasPasswordError()
    {
        string password = new('a', UserConstraints.MaxPasswordLength + 1);

        TestValidationResult<RegisterUserCommand> result =
            _validator.TestValidate(CreateCommand(password: password));

        result.ShouldHaveValidationErrorFor(command => command.Password)
            .WithErrorMessage(
                $"Password length must be between {UserConstraints.MinPasswordLength} and {UserConstraints.MaxPasswordLength} characters.");
    }

    private static RegisterUserCommand CreateCommand(
        string username = AuthTestData.Username,
        string password = AuthTestData.Password) =>
        new(username, password);
}
