using AutoMapper;
using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Products;

namespace NadinSoftTask.Application.Queries.ListProductsForUser;

public sealed class ListProductsForUserQueryHandler(
    IUserRepository userRepository,
    IProductRepository productRepository,
    IMapper mapper) : IRequestHandler<ListProductsForUserQuery, Result<IReadOnlyList<ProductDto>>>
{
    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(
        ListProductsForUserQuery request,
        CancellationToken cancellationToken)
    {
        UserDto? user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return new DomainError("User was not found.", DomainErrorKind.NotFound);
        }

        IReadOnlyList<Product> products = await productRepository.ListByOwnerAsync(
            request.UserId,
            cancellationToken);

        return Result<IReadOnlyList<ProductDto>>.Success(
            mapper.Map<IReadOnlyList<ProductDto>>(products));
    }
}
