using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NadinSoftTask.Api.OpenApi;

/// <summary>
/// Restricts JSON responses to the <c>application/json</c> media type instead of Swashbuckle's
/// default list of JSON-compatible media types.
/// </summary>
public sealed class JsonContentTypeDocumentFilter : IDocumentFilter
{
    private const string JsonContentType = "application/json";

    /// <inheritdoc />
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (OpenApiPathItem pathItem in swaggerDoc.Paths.Values)
        {
            foreach (OpenApiOperation operation in pathItem.Operations.Values)
            {
                foreach (OpenApiResponse response in operation.Responses.Values)
                {
                    if (!response.Content.TryGetValue(JsonContentType, out OpenApiMediaType? mediaType))
                    {
                        continue;
                    }

                    response.Content.Clear();
                    response.Content[JsonContentType] = mediaType;
                }
            }
        }
    }
}
