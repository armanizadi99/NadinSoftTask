using FluentValidation.TestHelper;
using NadinSoftTask.Application.Commands.DeleteProduct;
using Xunit;

namespace NadinSoftTask.Tests.Application.Validation;

public class DeleteProductCommandValidatorTests
{
    private readonly DeleteProductCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidId_HasNoErrors()
    {
        TestValidationResult<DeleteProductCommand> result =
            _validator.TestValidate(new DeleteProductCommand(1));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidId_HasIdError(int id)
    {
        TestValidationResult<DeleteProductCommand> result =
            _validator.TestValidate(new DeleteProductCommand(id));

        result.ShouldHaveValidationErrorFor(command => command.Id)
            .WithErrorMessage("Product id must be greater than zero.");
    }
}
