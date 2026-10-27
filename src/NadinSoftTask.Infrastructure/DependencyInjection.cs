using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NadinSoftTask.Application.Common;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Infrastructure.Identity;
using NadinSoftTask.Infrastructure.Persistence;
using NadinSoftTask.Infrastructure.Persistence.Repositories;
using NadinSoftTask.Infrastructure.Security;

namespace NadinSoftTask.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServer =>
            {
                sqlServer.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                sqlServer.EnableRetryOnFailure(3);
            }));

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<AppDbContext>());

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = UserConstraints.MinPasswordLength;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
                options.User.RequireUniqueEmail = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}
