using FinShark.Application.Auth.Services;
using FinShark.Application.Common;
using FinShark.Domain.Entities;
using FinShark.Domain.Repositories;
using FinShark.Persistence.Common;
using FinShark.Persistence.Repositories;
using FinShark.Persistence.Seeding;
using FinShark.Persistence.Services;
using Microsoft.AspNetCore.Identity;
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
        // Support using in-memory DB for tests.
        var useInMemoryDb = bool.TryParse(Environment.GetEnvironmentVariable("FINSHARK_USE_INMEMORY_DB"), out var inMemoryFlag) && inMemoryFlag
            || bool.TryParse(configuration["UseInMemoryDatabase"], out var configInMemory) && configInMemory;

        // Register interceptor for audit shadow properties.
        services.AddScoped<Audit.AuditSaveChangesInterceptor>();

        if (useInMemoryDb)
        {
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                options.UseInMemoryDatabase("FinSharkInMemoryDb");
                options.AddInterceptors(serviceProvider.GetRequiredService<Audit.AuditSaveChangesInterceptor>());
            });
        }
        else
        {
            // Priority: Environment Variable > appsettings.Development.json > throw error
            var connectionString = Environment.GetEnvironmentVariable("FINSHARK_DB_CONNECTION")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string not found. Set FINSHARK_DB_CONNECTION environment variable or configure 'DefaultConnection' in appsettings.Development.json");

            // Register DbContext
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
                options.AddInterceptors(serviceProvider.GetRequiredService<Audit.AuditSaveChangesInterceptor>());
            });
        }

        // Register Identity services
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;

            // Sign-in settings
            options.SignIn.RequireConfirmedEmail = false; // Set to true in production
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // Register repositories
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();

        // Register auth service
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IAppUrlProvider, AppUrlProvider>();

        // Register data seeder
        services.AddScoped<DataSeeder>();

        return services;
    }
}
