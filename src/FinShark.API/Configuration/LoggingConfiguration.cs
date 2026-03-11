using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinShark.API.Configuration;

/// <summary>
/// Logging configuration
/// Separated for clean dependency injection and maintainability
/// Supports different log levels per environment
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configures application-wide logging
    /// </summary>
    public static ILoggingBuilder AddApplicationLogging(
        this ILoggingBuilder logging,
        IConfiguration configuration)
    {
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        var logLevel = GetLogLevelForEnvironment(environment);

        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();

        // Only add EventLog on Windows
        if (OperatingSystem.IsWindows())
        {
            logging.AddEventLog();
        }

        logging.SetMinimumLevel(logLevel);

        return logging;
    }

    /// <summary>
    /// Determines log level based on environment
    /// </summary>
    private static LogLevel GetLogLevelForEnvironment(string environment) => environment switch
    {
        "Development" => LogLevel.Debug,
        "Staging" => LogLevel.Information,
        "Production" => LogLevel.Warning,
        _ => LogLevel.Information
    };
}
