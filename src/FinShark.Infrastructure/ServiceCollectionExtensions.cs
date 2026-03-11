using Microsoft.Extensions.DependencyInjection;

namespace FinShark.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection extensions
/// Registers infrastructure services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services)
    {
        // Add infrastructure services here
        // These are external service integrations (email, SMS, file storage, etc.)

        return services;
    }
}
