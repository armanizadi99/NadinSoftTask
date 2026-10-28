using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Api.Filters;

/// <summary>
/// Maps application <see cref="Result" /> values returned by controller actions into
/// HTTP success responses or RFC 7807 problem details.
/// </summary>
/// <remarks>
/// A result filter is used instead of middleware because middleware cannot inspect the
/// <see cref="IActionResult" /> produced by MVC actions. Controllers return
/// <c>Ok(result)</c>, <c>StatusCode(201, result)</c>, or <c>StatusCode(204, result)</c>;
/// this filter unwraps the result and replaces failures with problem details.
/// </remarks>
public sealed class ResultMappingFilter(ProblemDetailsFactory problemDetailsFactory)
    : IResultFilter
{
    private static readonly ConcurrentDictionary<Type, ResultPropertyAccessor> Accessors = new();

    /// <inheritdoc />
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not ObjectResult objectResult || objectResult.Value is null)
        {
            return;
        }

        if (!TryGetResult(objectResult.Value, out ResultSnapshot snapshot))
        {
            return;
        }

        if (snapshot.IsFailure)
        {
            context.Result = CreateProblemResult(
                context.HttpContext,
                snapshot.Error);

            return;
        }

        int statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;

        if (statusCode == StatusCodes.Status204NoContent)
        {
            context.Result = new NoContentResult();
            return;
        }

        if (snapshot.HasValue)
        {
            objectResult.Value = snapshot.Value;
            objectResult.StatusCode = statusCode;
            return;
        }

        // A successful non-generic Result with a status that should have no body.
        context.Result = new StatusCodeResult(statusCode);
    }

    /// <inheritdoc />
    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private ObjectResult CreateProblemResult(
        HttpContext httpContext,
        DomainError error)
    {
        int statusCode = MapStatusCode(error.Kind);

        ProblemDetails problemDetails;

        if (error.Kind == DomainErrorKind.Validation)
        {
            problemDetails = new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    ["general"] = [error.Message]
                })
            {
                Status = statusCode,
                Title = "One or more validation errors occurred.",
                Detail = error.Message,
                Instance = httpContext.Request.Path
            };
        }
        else
        {
            problemDetails = problemDetailsFactory.CreateProblemDetails(
                httpContext,
                statusCode: statusCode,
                title: ReasonPhrases.GetReasonPhrase(statusCode),
                detail: error.Message,
                instance: httpContext.Request.Path);
        }

        problemDetails.Extensions["errorKind"] = error.Kind.ToString();
        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    private static int MapStatusCode(DomainErrorKind kind) =>
        kind switch
        {
            DomainErrorKind.Validation => StatusCodes.Status400BadRequest,
            DomainErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
            DomainErrorKind.NotAuthorized => StatusCodes.Status403Forbidden,
            DomainErrorKind.NotFound => StatusCodes.Status404NotFound,
            DomainErrorKind.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

    private static bool TryGetResult(object value, out ResultSnapshot snapshot)
    {
        if (value is Result result)
        {
            snapshot = new ResultSnapshot(result.IsSuccess, result.Error, null, HasValue: false);
            return true;
        }

        Type type = value.GetType();

        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Result<>))
        {
            snapshot = default;
            return false;
        }

        ResultPropertyAccessor accessor = Accessors.GetOrAdd(
            type,
            static resultType => ResultPropertyAccessor.Create(resultType));

        bool isSuccess = (bool)accessor.IsSuccess.GetValue(value)!;
        DomainError error = (DomainError)accessor.Error.GetValue(value)!;
        object? resultValue = accessor.Value.GetValue(value);

        snapshot = new ResultSnapshot(isSuccess, error, resultValue, HasValue: true);
        return true;
    }

    private readonly record struct ResultSnapshot(
        bool IsSuccess,
        DomainError Error,
        object? Value,
        bool HasValue)
    {
        public bool IsFailure => !IsSuccess;
    }

    private sealed class ResultPropertyAccessor(
        PropertyInfo isSuccess,
        PropertyInfo error,
        PropertyInfo value)
    {
        public PropertyInfo IsSuccess { get; } = isSuccess;

        public PropertyInfo Error { get; } = error;

        public PropertyInfo Value { get; } = value;

        public static ResultPropertyAccessor Create(Type resultType) =>
            new(
                GetProperty(resultType, nameof(Result<object>.IsSuccess)),
                GetProperty(resultType, nameof(Result<object>.Error)),
                GetProperty(resultType, nameof(Result<object>.Value)));

        private static PropertyInfo GetProperty(Type resultType, string propertyName) =>
            resultType.GetProperty(propertyName)
            ?? throw new InvalidOperationException(
                $"The result type '{resultType}' does not define a '{propertyName}' property.");
    }
}
