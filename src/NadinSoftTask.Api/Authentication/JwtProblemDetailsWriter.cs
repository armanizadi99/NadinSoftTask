using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

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
        httpContext.Response.ContentType = "application/problem+json";

        ProblemDetails problemDetails = new()
        {
            Type = "about:blank",
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorKind"] =
            statusCode == StatusCodes.Status401Unauthorized ? "Unauthorized" : "Forbidden";
        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);
    }
}
