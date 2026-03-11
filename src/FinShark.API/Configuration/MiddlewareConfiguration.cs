using FinShark.API.Middleware;

namespace FinShark.API.Configuration;

/// <summary>
/// Custom middleware configuration
/// Separated for clean code organization and maintainability
/// </summary>
public static class MiddlewareConfiguration
{
    /// <summary>
    /// Registers all custom middleware in the correct order
    /// </summary>
    public static WebApplication UseCustomMiddleware(this WebApplication app)
    {
        // Custom Exception Handler Middleware
        // Must be early in the pipeline to catch exceptions from other middleware
        app.UseMiddleware<ExceptionMiddleware>();

        return app;
    }
}
