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

        // Health endpoint with SMTP config warning state
        app.MapGet("/api/health", (IConfiguration config) =>
        {
            var warnings = new List<string>();
            var smtpSection = config.GetSection("Smtp");
            var provider = smtpSection["Provider"];
            var host = smtpSection["Host"];
            var fromEmail = smtpSection["FromEmail"];
            var useDefaultCredentials = bool.TryParse(smtpSection["UseDefaultCredentials"], out var useDefault) ? useDefault : false;
            var userName = smtpSection["UserName"];
            var password = smtpSection["Password"];

            if (string.IsNullOrWhiteSpace(host))
            {
                warnings.Add("SMTP host is not configured. Email delivery is unavailable.");
            }

            if (string.IsNullOrWhiteSpace(fromEmail))
            {
                warnings.Add("SMTP FromEmail is not configured. Email 'From' address is required.");
            }

            if (!useDefaultCredentials)
            {
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                {
                    warnings.Add("SMTP credentials are missing. Set Smtp:UserName and Smtp:Password or enable UseDefaultCredentials.");
                }
            }

            var smtpConfigured = warnings.Count == 0;
            var healthData = new
            {
                status = smtpConfigured ? "Healthy" : "Degraded",
                smtp = new
                {
                    provider,
                    configured = smtpConfigured,
                    host,
                    fromEmail,
                    useDefaultCredentials,
                    warnings
                }
            };

            var message = smtpConfigured ? "API is healthy" : "API is healthy with warnings";
            return Results.Ok(FinShark.Application.Dtos.ApiResponse<object>.SuccessResponse(healthData, message));
        });

        // Map Controllers
        app.MapControllers();

        return app;
    }
}
