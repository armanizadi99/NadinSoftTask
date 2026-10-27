using NadinSoftTask.Application.Commands.CreateProduct;
using NadinSoftTask.Application.Commands.EditProduct;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Products;

namespace NadinSoftTask.Tests.Application.TestData;

internal static class ProductDtoFactory
{
    public static ProductDto From(Product product) =>
        new(
            product.Id,
            product.OwnerId,
            product.Name,
            product.ManufacturePhone.Value,
            product.ManufactureEmail.Value,
            product.IsAvailable,
            product.ProduceDate);

    public static ProductDto From(CreateProductCommand command, string ownerId, int id) =>
        new(
            id,
            ownerId,
            command.Name,
            command.ManufacturePhone,
            command.ManufactureEmail,
            command.IsAvailable!.Value,
            command.ProduceDate!.Value);

    public static ProductDto From(EditProductCommand command, string ownerId) =>
        new(
            command.Id,
            ownerId,
            command.Name,
            command.ManufacturePhone,
            command.ManufactureEmail,
            command.IsAvailable!.Value,
            command.ProduceDate!.Value);
}
