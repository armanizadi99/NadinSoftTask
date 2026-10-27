using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        IValidator<TRequest>[] validatorArray = validators.ToArray();

        if (validatorArray.Length == 0)
        {
            return await next(cancellationToken);
        }

        ValidationResult[] results = await Task.WhenAll(
            validatorArray.Select(validator => validator.ValidateAsync(request, cancellationToken)));

        ValidationFailure[] failures = results
            .SelectMany(result => result.Errors)
            .ToArray();

        if (failures.Length == 0)
        {
            return await next(cancellationToken);
        }

        DomainError error = new(
            string.Join("; ", failures.Select(failure => failure.ErrorMessage)),
            DomainErrorKind.Validation);

        return ResultFailureFactory.Create<TResponse>(error);
    }
}
