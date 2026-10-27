namespace NadinSoftTask.Api.Contracts.Products;

/// <summary>
/// Data required to create a product. The authenticated user becomes the owner.
/// </summary>
public sealed record CreateProductRequest
{
    /// <summary>
    /// The product name. Must be 3-150 characters and at most 300 UTF-8 bytes.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The manufacturer phone number. Must be 4-14 characters.
    /// </summary>
    public required string ManufacturePhone { get; init; }

    /// <summary>
    /// The manufacturer email address. Must be a valid email address of at most
    /// 254 UTF-8 bytes.
    /// </summary>
    public required string ManufactureEmail { get; init; }

    /// <summary>
    /// Whether the product is currently available.
    /// </summary>
    public required bool IsAvailable { get; init; }

    /// <summary>
    /// The date on which the product was produced, in ISO 8601 date format (yyyy-MM-dd).
    /// </summary>
    public required DateOnly ProduceDate { get; init; }
}
