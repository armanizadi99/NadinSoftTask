using FluentValidation.TestHelper;
using NadinSoftTask.Application.Commands.EditProduct;
using NadinSoftTask.Tests.Application.TestData;
using Xunit;

namespace NadinSoftTask.Tests.Application.Validation;

public class EditProductCommandValidatorTests
{
    private readonly EditProductCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        TestValidationResult<EditProductCommand> result = _validator.TestValidate(CreateCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidId_HasIdError(int id)
    {
        TestValidationResult<EditProductCommand> result =
            _validator.TestValidate(CreateCommand(id: id));

        result.ShouldHaveValidationErrorFor(command => command.Id)
            .WithErrorMessage("Product id must be greater than zero.");
    }

    [Fact]
    public void Validate_WithMissingName_HasNameError()
    {
        TestValidationResult<EditProductCommand> result =
            _validator.TestValidate(CreateCommand(name: string.Empty));

        result.ShouldHaveValidationErrorFor(command => command.Name);
    }

    [Fact]
    public void Validate_WithMissingIsAvailable_HasIsAvailableError()
    {
        TestValidationResult<EditProductCommand> result =
            _validator.TestValidate(CreateCommand(isAvailable: null));

        result.ShouldHaveValidationErrorFor(command => command.IsAvailable)
            .WithErrorMessage("IsAvailable is required.");
    }

    [Fact]
    public void Validate_WithMissingProduceDate_HasProduceDateError()
    {
        EditProductCommand command = new(
            1,
            ProductTestData.ProductName,
            ProductTestData.PhoneNumber,
            ProductTestData.EmailAddress,
            true,
            null);

        TestValidationResult<EditProductCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.ProduceDate)
            .WithErrorMessage("ProduceDate is required.");
    }

    private static EditProductCommand CreateCommand(
        int id = 1,
        string name = ProductTestData.ProductName,
        string phone = ProductTestData.PhoneNumber,
        string email = ProductTestData.EmailAddress,
        bool? isAvailable = true,
        DateOnly? produceDate = null) =>
        new(
            id,
            name,
            phone,
            email,
            isAvailable,
            produceDate ?? ProductTestData.ProduceDate);
}
