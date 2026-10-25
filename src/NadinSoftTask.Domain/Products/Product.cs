using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Domain.Products;

public sealed class Product
{
    private Product()
    {
    }

    private Product(
        Guid id,
        string ownerId,
        string name,
        Phone manufacturePhone,
        Email manufactureEmail,
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

    public Guid Id { get; private set; }

    public string OwnerId { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public Phone ManufacturePhone { get; private set; } = null!;

    public Email ManufactureEmail { get; private set; } = null!;

    public bool IsAvailable { get; private set; }

    public DateOnly ProduceDate { get; private set; }

    public static Result<Product> Create(
        string ownerId,
        string name,
        Phone manufacturePhone,
        Email manufactureEmail,
        bool isAvailable,
        DateOnly produceDate)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            throw new ArgumentException("Owner id cannot be empty or whitespace.", nameof(ownerId));
        }

        ValidateDetails(name, manufacturePhone, manufactureEmail);

        Product product = new(
            Guid.NewGuid(),
            ownerId,
            name,
            manufacturePhone,
            manufactureEmail,
            isAvailable,
            produceDate);

        return product;
    }

    public bool EnsureOwnedBy(string requesterId) =>
        string.Equals(OwnerId, requesterId, StringComparison.Ordinal);

    public Result Update(
        string requesterId,
        string name,
        Phone manufacturePhone,
        Email manufactureEmail,
        bool isAvailable,
        DateOnly produceDate)
    {
        throw new NotImplementedException();
    }

    private static void ValidateDetails(
        string name,
        Phone manufacturePhone,
        Email manufactureEmail)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name cannot be empty or whitespace.", nameof(name));
        }

        if (name.Length < ProductConstraints.MinNameLength || name.Length > ProductConstraints.MaxNameLength)
        {
            throw new ArgumentException(
                $"Product name length must be between {ProductConstraints.MinNameLength} and {ProductConstraints.MaxNameLength} characters.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(manufacturePhone);
        ArgumentNullException.ThrowIfNull(manufactureEmail);
    }
}
