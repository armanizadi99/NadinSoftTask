namespace NadinSoftTask.Application.Common.Dtos;

/// <summary>
/// A product returned by the API.
/// </summary>
public sealed record ProductDto
{
    /// <summary>
    /// Creates a product representation.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="ownerId">The identifier of the user who created the product.</param>
    /// <param name="name">The product name.</param>
    /// <param name="manufacturePhone">The manufacturer phone number.</param>
    /// <param name="manufactureEmail">The manufacturer email address.</param>
    /// <param name="isAvailable">Whether the product is currently available.</param>
    /// <param name="produceDate">The date on which the product was produced.</param>
    public ProductDto(
        int id,
        string ownerId,
        string name,
        string manufacturePhone,
        string manufactureEmail,
        bool isAvailable,
        DateOnly produceDate)
    {
        Id = id;
        OwnerId = ownerId;
        Name = name;
        ManufacturePhone = manufacturePhone;
        ManufactureEmail = manufactureEmail;
        IsAvailable = isAvailable;
        ProduceDate = produceDate;
    }

    /// <summary>
    /// The product identifier.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The identifier of the user who created the product.
    /// </summary>
    public string OwnerId { get; init; }

    /// <summary>
    /// The product name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The manufacturer phone number.
    /// </summary>
    public string ManufacturePhone { get; init; }

    /// <summary>
    /// The manufacturer email address.
    /// </summary>
    public string ManufactureEmail { get; init; }

    /// <summary>
    /// Whether the product is currently available.
    /// </summary>
    public bool IsAvailable { get; init; }

    /// <summary>
    /// The date on which the product was produced.
    /// </summary>
    public DateOnly ProduceDate { get; init; }
}
