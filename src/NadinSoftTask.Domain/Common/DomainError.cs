namespace NadinSoftTask.Domain.Common;

public sealed record DomainError
{
    public DomainError(string message, DomainErrorKind kind)
    {
        Message = message;
        Kind = kind;
    }

    public static DomainError None { get; } = new(string.Empty, DomainErrorKind.None);

    public string Message { get; }

    public DomainErrorKind Kind { get; }
}
