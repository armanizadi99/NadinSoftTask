using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NadinSoftTask.Api.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace NadinSoftTask.Api;

/// <summary>
/// Configures Swashbuckle/OpenAPI and the Swagger UI for the API.
/// </summary>
public static class ApiDocumentationExtensions
{
    private const string DocumentName = "v1";

    /// <summary>
    /// Registers the OpenAPI document generator and its XML documentation sources.
    /// </summary>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                DocumentName,
                new OpenApiInfo
                {
                    Title = "NadinSoftTask API",
                    Version = "v1",
                    Description =
                        "Product catalogue API with JWT authentication.\n\n" +
                        "### Authorizing requests\n" +
                        "1. Call `POST /api/auth/register` or `POST /api/auth/login`.\n" +
                        "2. Copy the `accessToken` value from the response.\n" +
                        "3. Click **Authorize** and paste the token **without** the `Bearer ` prefix.\n\n" +
                        "Access tokens expire after 15 minutes. Use `POST /api/auth/refresh` with the " +
                        "refresh token to rotate the token pair.\n\n" +
                        "### Anonymous endpoints\n" +
                        "- `GET /api/products`\n" +
                        "- `GET /api/products/by-user/{userId}`\n\n" +
                        "All other product endpoints require a valid Bearer token. Swagger UI is " +
                        "available in every environment."
                });

            options.AddSecurityDefinition(
                AuthorizeCheckOperationFilter.SecuritySchemeName,
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description =
                        "Paste the access token returned by register, login, or refresh. " +
                        "Swagger adds the `Bearer ` prefix automatically.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

            options.OperationFilter<AuthorizeCheckOperationFilter>();
            options.OperationFilter<ProblemDetailsContentTypeOperationFilter>();
            options.OperationFilter<EmptyResponseOperationFilter>();
            options.DocumentFilter<JsonContentTypeDocumentFilter>();
            options.SchemaFilter<ExampleSchemaFilter>();

            options.SupportNonNullableReferenceTypes();
            options.NonNullableReferenceTypesAsRequired();
            options.MapType<DateOnly>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date"
            });
            options.DescribeAllParametersInCamelCase();
            options.CustomSchemaIds(type => type.Name);
            options.OrderActionsBy(apiDescription =>
                $"{apiDescription.ActionDescriptor.RouteValues["controller"]}_{apiDescription.RelativePath}");

            IncludeXmlComments(options);
        });

        return services;
    }

    /// <summary>
    /// Adds the Swagger JSON endpoint and Swagger UI to the request pipeline.
    /// </summary>
    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{DocumentName}/swagger.json", "NadinSoftTask API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "NadinSoftTask API";
            options.EnablePersistAuthorization();
            options.EnableTryItOutByDefault();
            options.DisplayRequestDuration();
            options.DocExpansion(DocExpansion.List);
            options.DefaultModelsExpandDepth(1);
        });

        return app;
    }

    private static void IncludeXmlComments(SwaggerGenOptions options)
    {
        string[] documentationFiles =
        [
            "NadinSoftTask.Api.xml",
            "NadinSoftTask.Application.xml"
        ];

        foreach (string documentationFile in documentationFiles)
        {
            string path = Path.Combine(AppContext.BaseDirectory, documentationFile);

            if (File.Exists(path))
            {
                options.IncludeXmlComments(path, includeControllerXmlComments: true);
            }
        }
    }
}
