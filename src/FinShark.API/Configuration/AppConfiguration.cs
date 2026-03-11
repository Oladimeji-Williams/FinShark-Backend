namespace FinShark.API.Configuration;

/// <summary>
/// Application configuration setup
/// Separated for clean code organization and maintainability
/// </summary>
public static class AppConfiguration
{
    /// <summary>
    /// Adds app configuration from appsettings files and environment variables
    /// </summary>
    public static IConfigurationBuilder AddAppConfiguration(
        this IConfigurationBuilder config,
        IHostEnvironment environment)
    {
        config
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        return config;
    }

    /// <summary>
    /// Adds app configuration to ConfigurationManager (used in WebApplicationBuilder)
    /// </summary>
    public static IConfigurationManager AddAppConfiguration(
        this IConfigurationManager config,
        IHostEnvironment environment)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        var appConfig = builder.Build();

        // Copy all configuration to the ConfigurationManager
        foreach (var section in appConfig.AsEnumerable())
        {
            config[section.Key] = section.Value;
        }

        return config;
    }
}

/// <summary>
/// Default configuration values
/// These can be overridden in appsettings.json or environment variables
/// </summary>
public static class DefaultAppSettings
{
    public static readonly Dictionary<string, string> Defaults = new()
    {
        ["Cors:AllowedOrigins"] = "*",
        ["Cors:AllowAllMethods"] = "true",
        ["Cors:AllowAllHeaders"] = "true",
        ["ASPNETCORE_ENVIRONMENT"] = "Development"
    };
}
