using Microsoft.Extensions.DependencyInjection;

namespace NadinSoftTask.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(applicationAssembly));

        services.AddAutoMapper(_ => { }, applicationAssembly);

        return services;
    }
}
