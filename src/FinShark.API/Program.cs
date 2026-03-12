using FinShark.Application;
using FinShark.API.Configuration;
using FinShark.API.Serialization;
using FinShark.Infrastructure;
using FinShark.Persistence;
using DotNetEnv;
using Serilog;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;

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
builder.Services.AddInfrastructureServices();

// Data Access Layer Services
builder.Services.AddPersistenceServices(builder.Configuration);

// API Layer Services - Completely Separated Configuration
builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddOpenApiConfiguration();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.Converters.Add(new IndustryTypeJsonConverter());
        options.SerializerSettings.Converters.Add(new RatingJsonConverter());
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


