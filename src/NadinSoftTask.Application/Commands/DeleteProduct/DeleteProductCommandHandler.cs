using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Products;

namespace NadinSoftTask.Application.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(
        DeleteProductCommand request,
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

        productRepository.Delete(product);
        await unitOfWork.SaveChangesAsync();

        return Result.Success;
    }
}
