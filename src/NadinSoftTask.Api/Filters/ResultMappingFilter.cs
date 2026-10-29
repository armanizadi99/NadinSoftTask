using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using NadinSoftTask.Api.Errors;
using NadinSoftTask.Domain.Common;
using ApplicationResult = NadinSoftTask.Domain.Common.IResult;

namespace NadinSoftTask.Api.Filters;

/// <summary>
/// Maps application <see cref="ApplicationResult" /> values returned by controller actions into
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
    /// <inheritdoc />
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not ObjectResult objectResult || objectResult.Value is not ApplicationResult result)
        {
            return;
        }

        if (result.IsFailure)
        {
            context.Result = CreateProblemResult(context.HttpContext, result.Error);
            return;
        }

        int statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;

        if (statusCode == StatusCodes.Status204NoContent)
        {
            context.Result = new NoContentResult();
            return;
        }

        if (result.HasValue)
        {
            objectResult.Value = result.Value;
            objectResult.StatusCode = statusCode;
            return;
        }

        context.Result = new StatusCodeResult(statusCode);
    }

    /// <inheritdoc />
    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private ObjectResult CreateProblemResult(HttpContext httpContext, DomainError error)
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
                Type = ProblemDetailsDefaults.Type,
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
                type: ProblemDetailsDefaults.Type,
                detail: error.Message,
                instance: httpContext.Request.Path);
        }

        problemDetails.Extensions["errorKind"] = MapErrorKind(error.Kind);
        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode,
            ContentTypes = { ProblemDetailsDefaults.ContentType }
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

    private static string MapErrorKind(DomainErrorKind kind) =>
        kind switch
        {
            DomainErrorKind.Validation => ProblemDetailsErrorKinds.Validation,
            DomainErrorKind.Unauthorized => ProblemDetailsErrorKinds.Unauthorized,
            DomainErrorKind.NotAuthorized => ProblemDetailsErrorKinds.Forbidden,
            DomainErrorKind.NotFound => ProblemDetailsErrorKinds.NotFound,
            DomainErrorKind.Conflict => ProblemDetailsErrorKinds.Conflict,
            _ => ProblemDetailsErrorKinds.InternalServerError
        };
}
