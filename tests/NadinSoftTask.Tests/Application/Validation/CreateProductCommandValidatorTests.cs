using FluentValidation.TestHelper;
using NadinSoftTask.Application.Commands.CreateProduct;
using NadinSoftTask.Domain.Products;
using NadinSoftTask.Tests.Application.TestData;
using Xunit;

namespace NadinSoftTask.Tests.Application.Validation;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        TestValidationResult<CreateProductCommand> result = _validator.TestValidate(CreateCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingName_HasNameError(string? name)
    {
        TestValidationResult<CreateProductCommand> result =
            _validator.TestValidate(CreateCommand(name: name!));

        result.ShouldHaveValidationErrorFor(command => command.Name)
            .WithErrorMessage("Product name cannot be empty or whitespace.");
    }

    [Fact]
    public void Validate_WithTooShortName_HasNameError()
    {
        string name = new('a', ProductConstraints.MinNameLength - 1);

        TestValidationResult<CreateProductCommand> result =
            _validator.TestValidate(CreateCommand(name: name));

        result.ShouldHaveValidationErrorFor(command => command.Name)
            .WithErrorMessage(
                $"Product name length must be between {ProductConstraints.MinNameLength} and {ProductConstraints.MaxNameLength} characters.");
    }

    [Fact]
    public void Validate_WithTooLongName_HasNameError()
    {
        string name = new('a', ProductConstraints.MaxNameLength + 1);

        TestValidationResult<CreateProductCommand> result =
            _validator.TestValidate(CreateCommand(name: name));

        result.ShouldHaveValidationErrorFor(command => command.Name)
            .WithErrorMessage(
                $"Product name length must be between {ProductConstraints.MinNameLength} and {ProductConstraints.MaxNameLength} characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingPhone_HasPhoneError(string? phone)
    {
        TestValidationResult<CreateProductCommand> result =
            _validator.TestValidate(CreateCommand(phone: phone!));

        result.ShouldHaveValidationErrorFor(command => command.ManufacturePhone)
            .WithErrorMessage("Phone cannot be empty or whitespace.");
    }

    [Fact]
    public void Validate_WithTooShortPhone_HasPhoneError()
    {
        string phone = new('1', ProductConstraints.MinPhoneLength - 1);

        TestValidationResult<CreateProductCommand> result =
            _validator.TestValidate(CreateCommand(phone: phone));

        result.ShouldHaveValidationErrorFor(command => command.ManufacturePhone)
            .WithErrorMessage(
                $"Phone length must be between {ProductConstraints.MinPhoneLength} and {ProductConstraints.MaxPhoneLength} characters.");
    }

    [Fact]
    public void Validate_WithTooLongPhone_HasPhoneError()
    {
        string phone = new('1', ProductConstraints.MaxPhoneLength + 1);

        TestValidationResult<CreateProductCommand> result =
            _validator.TestValidate(CreateCommand(phone: phone));

        result.ShouldHaveValidationErrorFor(command => command.ManufacturePhone)
            .WithErrorMessage(
                $"Phone length must be between {ProductConstraints.MinPhoneLength} and {ProductConstraints.MaxPhoneLength} characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithMissingEmail_HasEmailError(string? email)
    {
        TestValidationResult<CreateProductCommand> result =
            _validator.TestValidate(CreateCommand(email: email!));

        result.ShouldHaveValidationErrorFor(command => command.ManufactureEmail)
            .WithErrorMessage("Email cannot be empty or whitespace.");
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("user@")]
    [InlineData("user@exa mple.com")]
    [InlineData("Bob <bob@example.com>")]
    [InlineData(" user@example.com")]
    public void Validate_WithInvalidEmail_HasEmailError(string email)
    {
        TestValidationResult<CreateProductCommand> result =
            _validator.TestValidate(CreateCommand(email: email));

        result.ShouldHaveValidationErrorFor(command => command.ManufactureEmail)
            .WithErrorMessage("Email is not a valid email address.");
    }

    [Fact]
    public void Validate_WithMissingIsAvailable_HasIsAvailableError()
    {
        TestValidationResult<CreateProductCommand> result =
            _validator.TestValidate(CreateCommand(isAvailable: null));

        result.ShouldHaveValidationErrorFor(command => command.IsAvailable)
            .WithErrorMessage("IsAvailable is required.");
    }

    [Fact]
    public void Validate_WithIsAvailableFalse_IsValid()
    {
        TestValidationResult<CreateProductCommand> result =
            _validator.TestValidate(CreateCommand(isAvailable: false));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMissingProduceDate_HasProduceDateError()
    {
        CreateProductCommand command = new(
            ProductTestData.ProductName,
            ProductTestData.PhoneNumber,
            ProductTestData.EmailAddress,
            true,
            null);

        TestValidationResult<CreateProductCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.ProduceDate)
            .WithErrorMessage("ProduceDate is required.");
    }

    [Fact]
    public void Validate_WithDefaultProduceDate_IsValid()
    {
        CreateProductCommand command = new(
            ProductTestData.ProductName,
            ProductTestData.PhoneNumber,
            ProductTestData.EmailAddress,
            true,
            DateOnly.MinValue);

        TestValidationResult<CreateProductCommand> result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static CreateProductCommand CreateCommand(
        string name = ProductTestData.ProductName,
        string phone = ProductTestData.PhoneNumber,
        string email = ProductTestData.EmailAddress,
        bool? isAvailable = true,
        DateOnly? produceDate = null) =>
        new(
            name,
            phone,
            email,
            isAvailable,
            produceDate ?? ProductTestData.ProduceDate);
}
