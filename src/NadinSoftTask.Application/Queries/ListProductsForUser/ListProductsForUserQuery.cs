using MediatR;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Queries.ListProductsForUser;

public sealed record ListProductsForUserQuery(string UserId) : IRequest<Result<IReadOnlyList<ProductDto>>>;
