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
            return new DomainError("Product was not found.", DomainErrorKind.NotFound);
        }

        if (!product.EnsureOwnedBy(currentUser.UserId))
        {
            return new DomainError(
                "Only the product creator can modify this product.",
                DomainErrorKind.NotAuthorized);
        }

        productRepository.Delete(product);
        await unitOfWork.SaveChangesAsync();

        return Result.Success;
    }
}
