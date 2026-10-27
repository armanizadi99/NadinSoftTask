using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NadinSoftTask.Api.OpenApi;

/// <summary>
/// Removes response content for status codes explicitly marked with
/// <see cref="ProducesEmptyResponseAttribute" />.
/// </summary>
public sealed class EmptyResponseOperationFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        IEnumerable<int> emptyStatusCodes = context.MethodInfo
            .GetCustomAttributes<ProducesEmptyResponseAttribute>(inherit: true)
            .Select(attribute => attribute.StatusCode);

        foreach (int statusCode in emptyStatusCodes)
        {
            if (operation.Responses.TryGetValue(statusCode.ToString(), out OpenApiResponse? response))
            {
                response.Content.Clear();
            }
        }
    }
}
