using FinShark.Application;
using FinShark.API.Configuration;
using FinShark.Infrastructure;
using FinShark.Persistence;

// ============================================
// Configuration Builder Setup
// ============================================

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


