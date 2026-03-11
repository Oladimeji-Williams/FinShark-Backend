using FinShark.Application;
using FinShark.API.Configuration;
using FinShark.Infrastructure;
using FinShark.Persistence;
using DotNetEnv;

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
builder.Services.AddControllers();

// Logging Configuration - Separated and Environment-Aware
builder.Logging.AddApplicationLogging(builder.Configuration);

// ============================================
// Build Application
// ============================================

var app = builder.Build();

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


