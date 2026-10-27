using Microsoft.Extensions.DependencyInjection;
using NadinSoftTask.Api.Authentication;
using NadinSoftTask.Api.Controllers;
using NadinSoftTask.Api.Filters;
using NadinSoftTask.Application.Common.Abstractions;

namespace NadinSoftTask.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddControllers(options => options.Filters.Add<ResultMappingFilter>())
            .AddApplicationPart(typeof(AuthController).Assembly);

        services.AddJwtAuthentication();
        services.AddApiDocumentation();

        return services;
    }
}
