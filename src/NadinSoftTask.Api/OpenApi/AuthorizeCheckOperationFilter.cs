using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NadinSoftTask.Api.OpenApi;

/// <summary>
/// Adds the Bearer security requirement to protected operations only, leaving
/// <see cref="AllowAnonymousAttribute" /> endpoints unsecured in the Swagger document.
/// </summary>
public sealed class AuthorizeCheckOperationFilter : IOperationFilter
{
    public const string SecuritySchemeName = "Bearer";

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        IList<object> metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        bool allowsAnonymous = metadata.OfType<IAllowAnonymous>().Any();
        bool requiresAuthentication = metadata.OfType<IAuthorizeData>().Any();

        if (allowsAnonymous || !requiresAuthentication)
        {
            return;
        }

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = SecuritySchemeName
                        }
                    }
                ] = Array.Empty<string>()
            }
        ];
    }
}
