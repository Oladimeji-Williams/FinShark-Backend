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
        var allowedOrigins = configuration["Cors:AllowedOrigins"]?.Split(',') 
            ?? new[] { "*" };

        services.AddCors(options =>
        {
            options.AddPolicy("AllowConfigured", policy =>
            {
                var method = configuration["Cors:AllowAllMethods"]?.ToLower() == "true" 
                    ? policy.AllowAnyMethod() 
                    : policy.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS");

                var headers = configuration["Cors:AllowAllHeaders"]?.ToLower() == "true"
                    ? method.AllowAnyHeader()
                    : method.WithHeaders("Content-Type", "Authorization");

                policy.WithOrigins(allowedOrigins)
                    .AllowCredentials();
            });
        });

        return services;
    }
}
