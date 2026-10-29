using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NadinSoftTask.Api.Errors;

namespace NadinSoftTask.Api.Authentication;

/// <summary>
/// Writes RFC 7807 responses for JWT authentication and authorization failures.
/// </summary>
internal static class JwtProblemDetailsWriter
{
    public static async Task WriteAsync(
        HttpContext httpContext,
        int statusCode,
        string detail,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = ProblemDetailsDefaults.ContentType;

        ProblemDetails problemDetails = new()
        {
            Type = ProblemDetailsDefaults.Type,
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorKind"] =
            statusCode == StatusCodes.Status401Unauthorized
                ? ProblemDetailsErrorKinds.Unauthorized
                : ProblemDetailsErrorKinds.Forbidden;
        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: ProblemDetailsDefaults.ContentType,
            cancellationToken: cancellationToken);
    }
}
