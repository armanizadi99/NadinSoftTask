using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NadinSoftTask.Application.Common.Security;

namespace NadinSoftTask.Api.Authentication;

/// <summary>
/// Registers JWT bearer authentication using the shared <see cref="JwtOptions" />.
/// </summary>
internal static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOptionsAccessor) =>
            {
                JwtOptions jwtOptions = jwtOptionsAccessor.Value;

                options.MapInboundClaims = false;
                options.IncludeErrorDetails = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                    NameClaimType = JwtRegisteredClaimNames.UniqueName,
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.Headers["WWW-Authenticate"] = "Bearer";

                        await JwtProblemDetailsWriter.WriteAsync(
                            context.HttpContext,
                            StatusCodes.Status401Unauthorized,
                            "Authentication is required to access this resource.",
                            context.HttpContext.RequestAborted);
                    },
                    OnForbidden = context =>
                        JwtProblemDetailsWriter.WriteAsync(
                            context.HttpContext,
                            StatusCodes.Status403Forbidden,
                            "You are not authorized to access this resource.",
                            context.HttpContext.RequestAborted)
                };
            });

        services.AddAuthorization();

        return services;
    }
}
