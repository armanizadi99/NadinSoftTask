using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NadinSoftTask.Api.Authentication;
using NadinSoftTask.Api.Errors;
using NadinSoftTask.Api.Filters;
using NadinSoftTask.Application.Common.Abstractions;

namespace NadinSoftTask.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddControllers(options => options.Filters.Add<ResultMappingFilter>());

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                Dictionary<string, string[]> errors = context.ModelState
                    .Where(entry => entry.Value is { Errors.Count: > 0 })
                    .GroupBy(entry => NormalizeModelStateKey(entry.Key))
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .SelectMany(entry => entry.Value!.Errors)
                            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? "The value is invalid."
                                : error.ErrorMessage)
                            .ToArray());

                ValidationProblemDetails problemDetails = new(errors)
                {
                    Type = ProblemDetailsDefaults.Type,
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred.",
                    Detail = "The request is invalid.",
                    Instance = context.HttpContext.Request.Path
                };

                problemDetails.Extensions["errorKind"] = ProblemDetailsErrorKinds.Validation;
                problemDetails.Extensions["traceId"] =
                    Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

                return new ObjectResult(problemDetails)
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentTypes = { ProblemDetailsDefaults.ContentType }
                };
            };
        });

        services.AddJwtAuthentication();
        services.AddApiDocumentation();

        return services;
    }

    private static string NormalizeModelStateKey(string key) =>
        key is "" or "$" or "request" ? "general" : key;
}
