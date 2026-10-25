#pragma warning disable CS9113 // Dependencies are read once the handler is implemented.

using AutoMapper;
using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;

namespace NadinSoftTask.Application.Queries.ListProductsForUser;

public sealed class ListProductsForUserQueryHandler(
    IProductRepository productRepository,
    IMapper mapper) : IRequestHandler<ListProductsForUserQuery, IReadOnlyList<ProductDto>>
{
    public Task<IReadOnlyList<ProductDto>> Handle(
        ListProductsForUserQuery request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
