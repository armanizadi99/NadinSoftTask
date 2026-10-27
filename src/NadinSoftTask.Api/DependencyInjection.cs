using Microsoft.Extensions.DependencyInjection;
using NadinSoftTask.Application.Common.Abstractions;

namespace NadinSoftTask.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}
