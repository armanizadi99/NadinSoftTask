using System.Reflection;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Common.Behaviors;

internal static class ResultFailureFactory
{
    public static TResponse Create<TResponse>(DomainError error)
    {
        Type responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type resultType = typeof(Result<>).MakeGenericType(responseType.GetGenericArguments()[0]);

            MethodInfo failureMethod = resultType.GetMethod(
                nameof(Result<object>.Failure),
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [typeof(DomainError)],
                modifiers: null)
                ?? throw new InvalidOperationException(
                    $"Response type '{responseType}' does not define a Failure factory.");

            return (TResponse)failureMethod.Invoke(null, [error])!;
        }

        throw new InvalidOperationException(
            $"The validation behavior only supports Result and Result<T> responses. Response type: '{responseType}'.");
    }
}
