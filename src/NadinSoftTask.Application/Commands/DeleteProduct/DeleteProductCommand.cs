using MediatR;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Commands.DeleteProduct;

public sealed record DeleteProductCommand(int Id) : IRequest<Result>;
