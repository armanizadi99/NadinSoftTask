namespace NadinSoftTask.Domain.Products;

public sealed record Phone
{
    public Phone(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Phone cannot be empty or whitespace.", nameof(value));
        }

        if (value.Length < ProductConstraints.MinPhoneLength || value.Length > ProductConstraints.MaxPhoneLength)
        {
            throw new ArgumentException(
                $"Phone length must be between {ProductConstraints.MinPhoneLength} and {ProductConstraints.MaxPhoneLength} characters.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
