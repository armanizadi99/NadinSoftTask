namespace NadinSoftTask.Domain.Products;

public static class ProductErrors
{
    public const string NotFound = "Product was not found.";

    public const string NotOwner = "Only the product creator can modify this product.";

    public const string DuplicateKey =
        "A product with the same produce date and manufacture email already exists.";
}
