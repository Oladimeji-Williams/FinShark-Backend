using FinShark.Application.Common;
using FinShark.Infrastructure.Email;
using FinShark.Infrastructure.FMP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinShark.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection extensions
/// Registers infrastructure services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var smtpSection = configuration.GetSection("Smtp");

        var smtpSettings = new SmtpSettings
        {
            Provider = smtpSection["Provider"],
            Host = smtpSection["Host"],
            Port = int.TryParse(smtpSection["Port"], out var port) ? port : 587,
            EnableSsl = bool.TryParse(smtpSection["EnableSsl"], out var enableSsl) ? enableSsl : true,
            UseDefaultCredentials = bool.TryParse(smtpSection["UseDefaultCredentials"], out var useDefaultCredentials) ? useDefaultCredentials : false,
            UserName = smtpSection["UserName"],
            Password = smtpSection["Password"],
            FromEmail = smtpSection["FromEmail"],
            FromName = smtpSection["FromName"],
            TimeoutInMilliseconds = int.TryParse(smtpSection["TimeoutInMilliseconds"], out var timeout) ? timeout : 15000
        };

        // Apply provider defaults if host/port is not explicitly set
        if (string.IsNullOrWhiteSpace(smtpSettings.Host))
        {
            var provider = smtpSettings.Provider?.Trim().ToLowerInvariant();
            if (provider == "gmail")
            {
                smtpSettings.Host = "smtp.gmail.com";
                smtpSettings.Port = 587;
                smtpSettings.EnableSsl = true;
            }
            else
            {
                // default to Mailtrap as dev fallback
                smtpSettings.Host = "smtp.mailtrap.io";
                smtpSettings.Port = 2525;
                smtpSettings.EnableSsl = true;
            }
        }

        services.AddSingleton(smtpSettings);

        if (!string.IsNullOrWhiteSpace(smtpSettings.Host) && !string.IsNullOrWhiteSpace(smtpSettings.FromEmail))
        {
            services.AddScoped<IEmailService, SmtpEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, NoOpEmailService>();
        }

        var fmpSection = configuration.GetSection("FMP");
        var fmpSettings = new FmpSettings
        {
            BaseUrl = fmpSection["BaseUrl"] ?? "https://financialmodelingprep.com",
            ApiKey = fmpSection["ApiKey"] ?? string.Empty,
            TimeoutSeconds = int.TryParse(fmpSection["TimeoutSeconds"], out var timeoutSec) ? timeoutSec : 30
        };

        services.AddSingleton(fmpSettings);
        services.AddHttpClient();
        services.AddScoped<IFmpService, FMPService>();

        return services;
    }
}
