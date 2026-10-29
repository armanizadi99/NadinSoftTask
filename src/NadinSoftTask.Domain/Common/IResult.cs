namespace NadinSoftTask.Domain.Common;

public interface IResult
{
    DomainError Error { get; }

    bool IsSuccess { get; }

    bool IsFailure { get; }

    bool HasValue { get; }

    object? Value { get; }
}
