using Microsoft.Extensions.DependencyInjection;

namespace FinShark.API.Configuration;

/// <summary>
/// OpenAPI (Swagger) configuration
/// Separated for clean dependency injection and maintainability
/// </summary>
public static class OpenApiConfiguration
{
    /// <summary>
    /// Configures OpenAPI/Swagger documentation
    /// </summary>
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi();

        return services;
    }
}



