using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NadinSoftTask.Application.Common.Behaviors;

namespace NadinSoftTask.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(applicationAssembly);

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(applicationAssembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddAutoMapper(_ => { }, applicationAssembly);

        return services;
    }
}
