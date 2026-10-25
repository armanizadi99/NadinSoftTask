using AutoMapper;
using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Products;

namespace NadinSoftTask.Application.Queries.ListAllProducts;

public sealed class ListAllProductsQueryHandler(
    IProductRepository productRepository,
    IMapper mapper) : IRequestHandler<ListAllProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(
        ListAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Product> products = await productRepository.ListAllAsync(cancellationToken);

        return mapper.Map<IReadOnlyList<ProductDto>>(products);
    }
}
