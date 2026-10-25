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
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            throw new ArgumentException("Owner id cannot be empty or whitespace.", nameof(ownerId));
        }

        Id = id;
        OwnerId = ownerId;

        SetName(name);
        SetManufacturePhone(manufacturePhone);
        SetManufactureEmail(manufactureEmail);

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
        if (!EnsureOwnedBy(requesterId))
        {
            return new DomainError(
                "Only the product creator can modify this product.",
                DomainErrorKind.NotAuthorized);
        }

        SetName(name);
        SetManufacturePhone(manufacturePhone);
        SetManufactureEmail(manufactureEmail);

        IsAvailable = isAvailable;
        ProduceDate = produceDate;

        return Result.Success;
    }

    private void SetName(string name)
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

        Name = name;
    }

    private void SetManufacturePhone(Phone manufacturePhone)
    {
        ArgumentNullException.ThrowIfNull(manufacturePhone);

        ManufacturePhone = manufacturePhone;
    }

    private void SetManufactureEmail(Email manufactureEmail)
    {
        ArgumentNullException.ThrowIfNull(manufactureEmail);

        ManufactureEmail = manufactureEmail;
    }
}
