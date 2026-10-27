using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Commands.EditProduct;

public sealed record EditProductCommand(
    int Id,
    string Name,
    string ManufacturePhone,
    string ManufactureEmail,
    bool? IsAvailable,
    DateOnly? ProduceDate) : IRequest<Result<ProductDto>>, IProductCommand;
