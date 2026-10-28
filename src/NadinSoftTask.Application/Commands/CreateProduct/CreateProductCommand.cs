using MediatR;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string ManufacturePhone,
    string ManufactureEmail,
    bool? IsAvailable,
    DateOnly? ProduceDate) : IRequest<Result<ProductDto>>;
