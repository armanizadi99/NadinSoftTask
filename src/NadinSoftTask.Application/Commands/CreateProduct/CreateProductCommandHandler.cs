#pragma warning disable CS9113 // Dependencies are read once the handler is implemented.

using AutoMapper;
using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMapper mapper) : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    public Task<Result<ProductDto>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
