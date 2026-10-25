#pragma warning disable CS9113 // Dependencies are read once the handler is implemented.

using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IRequestHandler<DeleteProductCommand, Result>
{
    public Task<Result> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
