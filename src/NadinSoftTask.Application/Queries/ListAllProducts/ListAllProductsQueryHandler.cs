#pragma warning disable CS9113 // Dependencies are read once the handler is implemented.

using AutoMapper;
using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;

namespace NadinSoftTask.Application.Queries.ListAllProducts;

public sealed class ListAllProductsQueryHandler(
    IProductRepository productRepository,
    IMapper mapper) : IRequestHandler<ListAllProductsQuery, IReadOnlyList<ProductDto>>
{
    public Task<IReadOnlyList<ProductDto>> Handle(
        ListAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
