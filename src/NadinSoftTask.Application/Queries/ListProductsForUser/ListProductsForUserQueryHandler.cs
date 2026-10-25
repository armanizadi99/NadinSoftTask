using AutoMapper;
using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Products;

namespace NadinSoftTask.Application.Queries.ListProductsForUser;

public sealed class ListProductsForUserQueryHandler(
    IProductRepository productRepository,
    IMapper mapper) : IRequestHandler<ListProductsForUserQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(
        ListProductsForUserQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Product> products = await productRepository.ListByOwnerAsync(
            request.UserId,
            cancellationToken);

        return mapper.Map<IReadOnlyList<ProductDto>>(products);
    }
}
