using MediatR;
using NadinSoftTask.Application.Common.Dtos;

namespace NadinSoftTask.Application.Queries.ListAllProducts;

public sealed record ListAllProductsQuery : IRequest<IReadOnlyList<ProductDto>>;
