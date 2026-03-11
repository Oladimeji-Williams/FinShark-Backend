namespace FinShark.API.Configuration;

/// <summary>
/// HTTP pipeline/middleware configuration
/// Separated for clean code organization and maintainability
/// </summary>
public static class PipelineConfiguration
{
    /// <summary>
    /// Configures the HTTP request pipeline
    /// </summary>
    public static WebApplication ConfigureHttpPipeline(this WebApplication app)
    {
        // Development-specific middleware
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseDeveloperExceptionPage();
        }

        // HTTPS Redirection (should not run in development with self-signed certs)
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // CORS (must be before auth and routing)
        app.UseCors("AllowConfigured");

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    /// <summary>
    /// Configures middleware and route handlers
    /// </summary>
    public static WebApplication ConfigureMiddlewareAndRoutes(
        this WebApplication app,
        IHostEnvironment environment)
    {
        using (var scope = app.Services.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Application environment: {Environment}", environment.EnvironmentName);
        }

        // Map Controllers
        app.MapControllers();

        return app;
    }
}
