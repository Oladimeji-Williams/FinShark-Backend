using FinShark.Domain.Repositories;
using FinShark.Persistence.Repositories;
using FinShark.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinShark.Persistence;

/// <summary>
/// Persistence layer dependency injection extensions
/// Registers database context and repository services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds persistence services to the dependency injection container
    /// Uses environment variables in production, appsettings in development
    /// </summary>
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Priority: Environment Variable > appsettings.Development.json > throw error
        var connectionString = Environment.GetEnvironmentVariable("FINSHARK_DB_CONNECTION")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string not found. Set FINSHARK_DB_CONNECTION environment variable or configure 'DefaultConnection' in appsettings.Development.json");

        // Register DbContext
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });

        // Register repositories
        services.AddScoped<IStockRepository, StockRepository>();

        // Register data seeder
        services.AddScoped<DataSeeder>();

        return services;
    }
}
