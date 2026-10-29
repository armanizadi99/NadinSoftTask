using NadinSoftTask.Domain.Products;

namespace NadinSoftTask.Tests.Application.TestData;

internal static class ProductTestData
{
    public const string OwnerId = "user-1";
    public const string OtherOwnerId = "user-2";
    public const string ProductName = "Acme Widget";
    public const string UpdatedProductName = "Acme Gadget";
    public const string PhoneNumber = "09123456789";
    public const string UpdatedPhoneNumber = "09120000000";
    public const string EmailAddress = "owner@acme.com";
    public const string UpdatedEmailAddress = "updated@acme.com";

    public static readonly DateOnly ProduceDate = new(2026, 1, 15);
    public static readonly DateOnly UpdatedProduceDate = new(2027, 2, 20);

    public static Product Create(
        int id = 1,
        string ownerId = OwnerId,
        string name = ProductName,
        string phoneNumber = PhoneNumber,
        string emailAddress = EmailAddress,
        bool isAvailable = true,
        DateOnly? produceDate = null)
    {
        Product product = Product.Create(
            ownerId,
            name,
            new Phone(phoneNumber),
            new Email(emailAddress),
            isAvailable,
            produceDate ?? ProduceDate);

        SetId(product, id);

        return product;
    }

    public static void SetId(Product product, int id)
    {
        typeof(Product)
            .GetProperty(nameof(Product.Id))!
            .SetValue(product, id);
    }
}
