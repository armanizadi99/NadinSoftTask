#pragma warning disable CS9113 // Dependencies are read once the handler is implemented.

using AutoMapper;
using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Commands.EditProduct;

public sealed class EditProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMapper mapper) : IRequestHandler<EditProductCommand, Result<ProductDto>>
{
    public Task<Result<ProductDto>> Handle(
        EditProductCommand request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
