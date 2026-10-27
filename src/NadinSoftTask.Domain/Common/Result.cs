namespace NadinSoftTask.Domain.Common;

public sealed class Result
{
    private Result(DomainError error)
    {
        Error = error;
    }

    public static Result Success { get; } = new(DomainError.None);

    public static Result Failure(DomainError error) =>
        new(error ?? throw new ArgumentNullException(nameof(error)));

    public DomainError Error { get; }

    public bool IsSuccess => Error == DomainError.None;

    public bool IsFailure => !IsSuccess;

    public static implicit operator Result(DomainError error) =>
        new(error ?? throw new ArgumentNullException(nameof(error)));
}

public sealed class Result<T>
{
    private Result(T? value, DomainError error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }

    public DomainError Error { get; }

    public bool IsSuccess => Error == DomainError.None;

    public bool IsFailure => !IsSuccess;

    public static Result<T> Success(T value) =>
        new(value, DomainError.None);

    public static Result<T> Failure(DomainError error) =>
        new(default, error ?? throw new ArgumentNullException(nameof(error)));

    public static implicit operator Result<T>(T value) =>
        new(value, DomainError.None);

    public static implicit operator Result<T>(DomainError error) =>
        new(default, error ?? throw new ArgumentNullException(nameof(error)));
}
