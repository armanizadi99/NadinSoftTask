using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NadinSoftTask.Api.OpenApi;

/// <summary>
/// Restricts error responses that carry problem details to the
/// <c>application/problem+json</c> media type and adds a status-specific example.
/// </summary>
public sealed class ProblemDetailsContentTypeOperationFilter : IOperationFilter
{
    private const string ProblemDetailsContentType = "application/problem+json";
    private const string ProblemDetailsSchemaId = "ProblemDetails";

    private static readonly IReadOnlyDictionary<int, (string Detail, string ErrorKind)> Examples =
        new Dictionary<int, (string Detail, string ErrorKind)>
        {
            [400] = ("The request could not be processed.", "BadRequest"),
            [401] = ("Authentication is required to access this resource.", "Unauthorized"),
            [403] = ("You are not authorized to access this resource.", "Forbidden"),
            [404] = ("The requested resource was not found.", "NotFound"),
            [409] = ("A resource with the same key already exists.", "Conflict"),
            [413] = ("The request body is too large.", "BadRequest"),
            [500] = ("An unexpected error occurred while processing the request.", "InternalServerError")
        };

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (KeyValuePair<string, OpenApiResponse> response in operation.Responses)
        {
            if (!int.TryParse(response.Key, out int statusCode) || statusCode < 400)
            {
                continue;
            }

            KeyValuePair<string, OpenApiMediaType> problemContent = response.Value.Content
                .FirstOrDefault(content => IsProblemDetails(content.Value.Schema));

            if (problemContent.Key is null)
            {
                continue;
            }

            OpenApiMediaType mediaType = problemContent.Value;
            response.Value.Content.Clear();
            response.Value.Content[ProblemDetailsContentType] = mediaType;

            if (mediaType.Schema?.Reference?.Id == ProblemDetailsSchemaId &&
                Examples.TryGetValue(statusCode, out (string Detail, string ErrorKind) example))
            {
                mediaType.Example = CreateExample(
                    statusCode,
                    example.Detail,
                    example.ErrorKind,
                    $"/{context.ApiDescription.RelativePath}");
            }
        }
    }

    private static OpenApiObject CreateExample(
        int statusCode,
        string detail,
        string errorKind,
        string instance) =>
        new()
        {
            ["type"] = new OpenApiString("about:blank"),
            ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(statusCode)),
            ["status"] = new OpenApiInteger(statusCode),
            ["detail"] = new OpenApiString(detail),
            ["instance"] = new OpenApiString(instance),
            ["errorKind"] = new OpenApiString(errorKind),
            ["traceId"] = new OpenApiString(
                "00-8a2b6f5c1d9e4a7b8c0d1e2f3a4b5c6d-1a2b3c4d5e6f7a8b-00")
        };

    private static bool IsProblemDetails(OpenApiSchema? schema) =>
        schema?.Reference?.Id is ProblemDetailsSchemaId or "ValidationProblemDetails";
}
