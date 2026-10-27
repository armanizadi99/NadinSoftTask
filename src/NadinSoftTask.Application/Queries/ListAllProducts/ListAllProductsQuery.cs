using MediatR;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Queries.ListAllProducts;

public sealed record ListAllProductsQuery : IRequest<Result<IReadOnlyList<ProductDto>>>;
