using MediatR;
using NadinSoftTask.Application.Common.Dtos;

namespace NadinSoftTask.Application.Queries.ListProductsForUser;

public sealed record ListProductsForUserQuery(string UserId) : IRequest<IReadOnlyList<ProductDto>>;
