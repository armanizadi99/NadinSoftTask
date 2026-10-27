namespace NadinSoftTask.Domain.Products;

public static class ProductConstraints
{
    public const int MinNameLength = 3;

    public const int MaxNameLength = 150;

    public const int MaxNameUtf8Bytes = 300;

    public const int MinPhoneLength = 4;

    public const int MaxPhoneLength = 14;

    public const int MaxEmailUtf8Bytes = 254;
}
