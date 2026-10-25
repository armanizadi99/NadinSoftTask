namespace NadinSoftTask.Application.Common.Dtos;

public sealed record ProductDto(
    int Id,
    string OwnerId,
    string Name,
    string ManufacturePhone,
    string ManufactureEmail,
    bool IsAvailable,
    DateOnly ProduceDate);
