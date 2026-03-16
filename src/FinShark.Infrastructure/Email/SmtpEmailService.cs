using FinShark.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace FinShark.Infrastructure.Email;

public sealed class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(SmtpSettings settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            _logger.LogWarning("SMTP settings are not configured. Email send skipped to {Email}", to);
            return;
        }

        if (!_settings.UseDefaultCredentials && (string.IsNullOrWhiteSpace(_settings.UserName) || string.IsNullOrWhiteSpace(_settings.Password)))
        {
            _logger.LogWarning("SMTP credentials are missing. Email send skipped to {Email}", to);
            return;
        }

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            UseDefaultCredentials = _settings.UseDefaultCredentials,
            Timeout = _settings.TimeoutInMilliseconds > 0 ? _settings.TimeoutInMilliseconds : 15000,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!_settings.UseDefaultCredentials)
        {
            client.Credentials = new NetworkCredential(_settings.UserName!, _settings.Password!);
        }

        var fromName = string.IsNullOrWhiteSpace(_settings.FromName) ? "FinShark" : _settings.FromName;
        var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(to);

        try
        {
            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Email} via SMTP host {Host}:{Port}", to, _settings.Host, _settings.Port);
        }
        catch (SmtpFailedRecipientException ex)
        {
            _logger.LogWarning(ex, "Failed sending email to recipient {Email} using SMTP host {Host}:{Port}", to, _settings.Host, _settings.Port);
            throw;
        }
        catch (SmtpException ex)
        {
            _logger.LogWarning(ex, "SMTP failure sending email to {Email}. Host={Host}:{Port}, UserName={UserName}", to, _settings.Host, _settings.Port, _settings.UserName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email to {Email}" , to);
            throw;
        }
    }
}
