using FinShark.Application;
using FinShark.API.Configuration;
using FinShark.API.Serialization;
using FinShark.Infrastructure;
using FinShark.Persistence;
using DotNetEnv;
using Serilog;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using FinShark.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

// ============================================
// Serilog Configuration
// ============================================

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/finshark-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("FinShark application starting up");

// ============================================
// Configuration Builder Setup
// ============================================

// Load environment variables from .env file in development
if (!Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Production", StringComparison.OrdinalIgnoreCase) ?? true)
{
    // Search for .env in current directory and up to 3 levels up the directory tree
    var currentDir = Directory.GetCurrentDirectory();
    var envPath = Path.Combine(currentDir, ".env");
    
    if (!File.Exists(envPath))
    {
        envPath = Path.Combine(currentDir, "..", ".env");
    }
    if (!File.Exists(envPath))
    {
        envPath = Path.Combine(currentDir, "..", "..", ".env");
    }
    if (!File.Exists(envPath))
    {
        envPath = Path.Combine(currentDir, "..", "..", "..", ".env");
    }
    
    if (File.Exists(envPath))
    {
        Env.Load(envPath);
    }
}

var builder = WebApplication.CreateBuilder(args);

var useInMemoryDb = bool.TryParse(Environment.GetEnvironmentVariable("FINSHARK_USE_INMEMORY_DB"), out var inMemoryFlag) && inMemoryFlag
    || bool.TryParse(builder.Configuration["UseInMemoryDatabase"], out var configInMemory) && configInMemory;

if (builder.Environment.IsDevelopment() || useInMemoryDb)
{
    var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtection-Keys");
    Directory.CreateDirectory(dataProtectionKeysPath);

    builder.Services.AddDataProtection()
        .SetApplicationName("FinShark")
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
}

// Configure Serilog as the logging provider
builder.Host.UseSerilog();

// ASP.NET Core automatically loads:
// - appsettings.json
// - appsettings.{Environment}.json
// - Environment variables
// So no explicit configuration loading needed

// ============================================
// Services Configuration (Dependency Injection)
// ============================================

// Business Logic Layer Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Data Access Layer Services
builder.Services.AddPersistenceServices(builder.Configuration);

// Current user context for audit and persistence
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<FinShark.Application.Common.ICurrentUserService, FinShark.API.Services.CurrentUserService>();

// Authentication & Authorization
builder.Services.AddAuthenticationConfiguration(builder.Configuration);

// API Layer Services - Completely Separated Configuration
builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddOpenApiConfiguration();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.Converters.Add(new SectorTypeJsonConverter());
        options.SerializerSettings.Converters.Add(new RatingJsonConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .Select(entry =>
                string.IsNullOrWhiteSpace(entry.Key)
                    ? "Invalid request payload."
                    : $"Invalid value for '{entry.Key}'.")
            .Distinct()
            .ToArray();

        if (errors.Length == 0)
        {
            errors = new[] { "Invalid request payload." };
        }

        var response = FinShark.Application.Dtos.ApiResponse<object>.FailureResponse("Validation failed", errors);
        return new BadRequestObjectResult(response);
    };
});

// ============================================
// Build Application
// ============================================

var app = builder.Build();

// ============================================
// Database Seeding (if --seed flag is provided)
// ============================================

if (args.Contains("--seed"))
{
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<FinShark.Persistence.Seeding.DataSeeder>();
        await seeder.SeedAsync();
    }
}

// ============================================
// HTTP Pipeline Configuration
// ============================================

// Configure HTTP request pipeline
app
    .ConfigureHttpPipeline()
    .UseCustomMiddleware()
    .ConfigureMiddlewareAndRoutes(app.Environment);

// ============================================
// Run Application
// ============================================

app.Run();
}
catch (HostAbortedException)
{
    // EF Core tools intentionally abort the host after design-time services are resolved.
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }


