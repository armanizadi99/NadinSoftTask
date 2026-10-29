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
            return new DomainError(ProductErrors.NotFound, DomainErrorKind.NotFound);
        }

        if (!product.EnsureOwnedBy(currentUser.UserId))
        {
            return new DomainError(ProductErrors.NotOwner, DomainErrorKind.NotAuthorized);
        }

        Email manufactureEmail = new(request.ManufactureEmail);

        bool keyExists = await productRepository.ExistsByProduceDateAndEmailAsync(
            request.ProduceDate!.Value,
            manufactureEmail,
            request.Id);

        if (keyExists)
        {
            return new DomainError(ProductErrors.DuplicateKey, DomainErrorKind.Conflict);
        }

        Result updateResult = product.Update(
            currentUser.UserId,
            request.Name,
            new Phone(request.ManufacturePhone),
            manufactureEmail,
            request.IsAvailable!.Value,
            request.ProduceDate!.Value);

        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        await unitOfWork.SaveChangesAsync();

        return mapper.Map<ProductDto>(product);
    }
}
