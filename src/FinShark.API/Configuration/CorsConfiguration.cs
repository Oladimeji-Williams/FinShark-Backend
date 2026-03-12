using Microsoft.Extensions.DependencyInjection;

namespace FinShark.API.Configuration;

/// <summary>
/// CORS (Cross-Origin Resource Sharing) configuration
/// Separated for clean dependency injection and maintainability
/// </summary>
public static class CorsConfiguration
{
    /// <summary>
    /// Configures CORS policies
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration["Cors:AllowedOrigins"]?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();
        var allowAnyOrigin = allowedOrigins.Length == 0 || allowedOrigins.Contains("*");
        var allowCredentials = configuration["Cors:AllowCredentials"]?.ToLower() == "true";

        services.AddCors(options =>
        {
            options.AddPolicy("AllowConfigured", policy =>
            {
                var builder = policy;

                if (allowAnyOrigin)
                {
                    builder = builder.AllowAnyOrigin();
                }
                else
                {
                    builder = builder.WithOrigins(allowedOrigins);
                    if (allowCredentials)
                    {
                        builder = builder.AllowCredentials();
                    }
                }

                builder = configuration["Cors:AllowAllMethods"]?.ToLower() == "true"
                    ? builder.AllowAnyMethod()
                    : builder.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS");

                _ = configuration["Cors:AllowAllHeaders"]?.ToLower() == "true"
                    ? builder.AllowAnyHeader()
                    : builder.WithHeaders("Content-Type", "Authorization");
            });
        });

        return services;
    }
}
