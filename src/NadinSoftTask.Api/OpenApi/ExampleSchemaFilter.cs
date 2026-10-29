using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NadinSoftTask.Api.Contracts.Auth;
using NadinSoftTask.Api.Contracts.Products;
using NadinSoftTask.Api.Errors;
using NadinSoftTask.Application.Common.Dtos;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NadinSoftTask.Api.OpenApi;

/// <summary>
/// Adds lightweight request and response examples to the generated OpenAPI schemas.
/// </summary>
public sealed class ExampleSchemaFilter : ISchemaFilter
{
    private const string SampleUserId = "3f2504e0-4f89-41d3-9a0c-0305e82c3301";

    /// <inheritdoc />
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(ProblemDetails) || context.Type == typeof(ValidationProblemDetails))
        {
            schema.Properties["errorKind"] = new OpenApiSchema
            {
                Type = "string",
                Description = "The application error kind."
            };
            schema.Properties["traceId"] = new OpenApiSchema
            {
                Type = "string",
                Description = "The trace identifier for the request."
            };
        }

        if (context.Type == typeof(RegisterRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["username"] = new OpenApiString("john_doe"),
                ["password"] = new OpenApiString("Passw0rd!")
            };
        }
        else if (context.Type == typeof(LoginRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["username"] = new OpenApiString("john_doe"),
                ["password"] = new OpenApiString("Passw0rd!")
            };
        }
        else if (context.Type == typeof(RefreshTokenRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["refreshToken"] = new OpenApiString("Q2xvdWQtcmVmcmVzaC10b2tlbi1leGFtcGxl")
            };
        }
        else if (context.Type == typeof(CreateProductRequest) ||
                 context.Type == typeof(UpdateProductRequest))
        {
            schema.Example = CreateProductExample();
        }
        else if (context.Type == typeof(AuthResult))
        {
            schema.Example = new OpenApiObject
            {
                ["accessToken"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."),
                ["refreshToken"] = new OpenApiString("Q2xvdWQtcmVmcmVzaC10b2tlbi1leGFtcGxl"),
                ["accessTokenExpiresAt"] = new OpenApiString("2026-10-27T12:15:00Z"),
                ["refreshTokenExpiresAt"] = new OpenApiString("2026-11-03T12:00:00Z"),
                ["userId"] = new OpenApiString(SampleUserId),
                ["username"] = new OpenApiString("john_doe")
            };
        }
        else if (context.Type == typeof(ProductDto))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiInteger(1),
                ["ownerId"] = new OpenApiString(SampleUserId),
                ["name"] = new OpenApiString("Wireless Mouse"),
                ["manufacturePhone"] = new OpenApiString("09123456789"),
                ["manufactureEmail"] = new OpenApiString("manufacturer@example.com"),
                ["isAvailable"] = new OpenApiBoolean(true),
                ["produceDate"] = new OpenApiString("2026-10-27")
            };
        }
        else if (context.Type == typeof(ValidationProblemDetails))
        {
            schema.Example = new OpenApiObject
            {
                ["type"] = new OpenApiString(ProblemDetailsDefaults.Type),
                ["title"] = new OpenApiString("One or more validation errors occurred."),
                ["status"] = new OpenApiInteger(400),
                ["detail"] = new OpenApiString("Product name cannot be empty or whitespace."),
                ["instance"] = new OpenApiString("/api/products"),
                ["errors"] = new OpenApiObject
                {
                    ["general"] = new OpenApiArray
                    {
                        new OpenApiString("Product name cannot be empty or whitespace.")
                    }
                },
                ["errorKind"] = new OpenApiString(ProblemDetailsErrorKinds.Validation),
                ["traceId"] = new OpenApiString("00-8a2b6f5c1d9e4a7b8c0d1e2f3a4b5c6d-1a2b3c4d5e6f7a8b-00")
            };
        }
    }

    private static OpenApiObject CreateProductExample() =>
        new()
        {
            ["name"] = new OpenApiString("Wireless Mouse"),
            ["manufacturePhone"] = new OpenApiString("09123456789"),
            ["manufactureEmail"] = new OpenApiString("manufacturer@example.com"),
            ["isAvailable"] = new OpenApiBoolean(true),
            ["produceDate"] = new OpenApiString("2026-10-27")
        };
}
