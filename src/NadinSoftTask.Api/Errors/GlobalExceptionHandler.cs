using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NadinSoftTask.Api.Errors;

/// <summary>
/// Converts unhandled exceptions into RFC 7807 problem details.
/// </summary>
/// <remarks>
/// Expected failures are represented by <see cref="NadinSoftTask.Domain.Common.Result" /> values and
/// are handled by <see cref="Filters.ResultMappingFilter" />. This handler is the last-resort
/// boundary for exceptions that escape the request pipeline.
/// </remarks>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const string UnexpectedErrorDetail =
        "An unexpected error occurred while processing the request.";

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // A client disconnect is not a server error and must not produce a response.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            return false;
        }

        int statusCode = StatusCodes.Status500InternalServerError;

        if (exception is BadHttpRequestException badHttpRequestException &&
            badHttpRequestException.StatusCode is >= 400 and < 600)
        {
            statusCode = badHttpRequestException.StatusCode;
        }

        string traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        bool isServerError = statusCode >= StatusCodes.Status500InternalServerError;

        if (isServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}. TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                traceId);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Rejected request while processing {Method} {Path}. TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                traceId);
        }

        httpContext.Response.StatusCode = statusCode;

        ProblemDetails problemDetails = new()
        {
            Type = ProblemDetailsDefaults.Type,
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = isServerError && !environment.IsDevelopment()
                ? UnexpectedErrorDetail
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorKind"] = isServerError
            ? ProblemDetailsErrorKinds.InternalServerError
            : ProblemDetailsErrorKinds.BadRequest;
        problemDetails.Extensions["traceId"] = traceId;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }
}
