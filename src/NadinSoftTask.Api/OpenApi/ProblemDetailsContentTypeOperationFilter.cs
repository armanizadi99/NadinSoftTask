using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NadinSoftTask.Api.OpenApi;

/// <summary>
/// Restricts error responses that carry problem details to the
/// <c>application/problem+json</c> media type.
/// </summary>
public sealed class ProblemDetailsContentTypeOperationFilter : IOperationFilter
{
    private const string ProblemDetailsContentType = "application/problem+json";

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
        }
    }

    private static bool IsProblemDetails(OpenApiSchema? schema) =>
        schema?.Reference?.Id is "ProblemDetails" or "ValidationProblemDetails";
}
