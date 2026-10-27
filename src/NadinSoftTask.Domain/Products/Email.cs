using System.Net.Mail;

namespace NadinSoftTask.Domain.Products;

public sealed record Email
{
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email cannot be empty or whitespace.", nameof(value));
        }

        if (!IsValid(value))
        {
            throw new ArgumentException("Email is not a valid email address.", nameof(value));
        }

        Value = value;
    }

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return MailAddress.TryCreate(value, out MailAddress? address) &&
               string.Equals(address.Address, value, StringComparison.Ordinal);
    }

    public string Value { get; }

    public override string ToString() => Value;
}
