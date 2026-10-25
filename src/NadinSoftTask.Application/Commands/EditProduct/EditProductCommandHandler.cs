using AutoMapper;
using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Products;

namespace NadinSoftTask.Application.Commands.EditProduct;

public sealed class EditProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMapper mapper) : IRequestHandler<EditProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(
        EditProductCommand request,
        CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetByIdAsync(request.Id);

        if (product is null)
        {
            return new DomainError("Product was not found.", DomainErrorKind.NotFound);
        }

        Email manufactureEmail = new(request.ManufactureEmail);

        bool keyExists = await productRepository.ExistsByProduceDateAndEmailAsync(
            request.ProduceDate,
            manufactureEmail,
            request.Id);

        if (keyExists)
        {
            return new DomainError(
                "A product with the same produce date and manufacture email already exists.",
                DomainErrorKind.Conflict);
        }

        Result updateResult = product.Update(
            currentUser.UserId,
            request.Name,
            new Phone(request.ManufacturePhone),
            manufactureEmail,
            request.IsAvailable,
            request.ProduceDate);

        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        await unitOfWork.SaveChangesAsync();

        return mapper.Map<ProductDto>(product);
    }
}
