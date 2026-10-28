using System.Text;
using FluentValidation;
using NadinSoftTask.Domain.Products;

namespace NadinSoftTask.Application.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Product name cannot be empty or whitespace.")
            .Length(ProductConstraints.MinNameLength, ProductConstraints.MaxNameLength)
            .WithMessage(
                $"Product name length must be between {ProductConstraints.MinNameLength} and {ProductConstraints.MaxNameLength} characters.")
            .Must(name => Encoding.UTF8.GetByteCount(name) <= ProductConstraints.MaxNameUtf8Bytes)
            .WithMessage(
                $"Product name must be at most {ProductConstraints.MaxNameUtf8Bytes} UTF-8 bytes.");

        RuleFor(command => command.ManufacturePhone)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Phone cannot be empty or whitespace.")
            .Length(ProductConstraints.MinPhoneLength, ProductConstraints.MaxPhoneLength)
            .WithMessage(
                $"Phone length must be between {ProductConstraints.MinPhoneLength} and {ProductConstraints.MaxPhoneLength} characters.");

        RuleFor(command => command.ManufactureEmail)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email cannot be empty or whitespace.")
            .Must(value => Email.IsValid(value))
            .WithMessage("Email is not a valid email address.");

        RuleFor(command => command.IsAvailable)
            .NotNull()
            .WithMessage("IsAvailable is required.");

        RuleFor(command => command.ProduceDate)
            .NotNull()
            .WithMessage("ProduceDate is required.");
    }
}
