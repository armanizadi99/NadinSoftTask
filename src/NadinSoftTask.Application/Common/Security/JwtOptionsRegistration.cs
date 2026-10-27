using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NadinSoftTask.Application.Common.Security;

public static class JwtOptionsRegistration
{
    /// <summary>
    /// Binds and validates <see cref="JwtOptions" /> once for every layer that consumes it.
    /// Token generation (Infrastructure) and token validation (Api) both read the same instance,
    /// so binding lives in the composition root instead of in either layer.
    /// </summary>
    public static IServiceCollection AddJwtOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Issuer),
                "Jwt:Issuer is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Audience),
                "Jwt:Audience is required.")
            .Validate(
                options => Encoding.UTF8.GetByteCount(options.SigningKey) >= 32,
                "Jwt:SigningKey must be at least 32 bytes.")
            .Validate(
                options => options.AccessTokenMinutes > 0,
                "Jwt:AccessTokenMinutes must be greater than zero.")
            .Validate(
                options => options.RefreshTokenDays > 0,
                "Jwt:RefreshTokenDays must be greater than zero.")
            .ValidateOnStart();

        return services;
    }
}
