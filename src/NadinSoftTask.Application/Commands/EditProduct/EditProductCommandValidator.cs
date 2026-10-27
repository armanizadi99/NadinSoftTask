using FluentValidation;
using NadinSoftTask.Application.Common.Validation;

namespace NadinSoftTask.Application.Commands.EditProduct;

public sealed class EditProductCommandValidator : ProductCommandValidator<EditProductCommand>
{
    public EditProductCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage("Product id must be greater than zero.");
    }
}
