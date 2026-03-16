using FinShark.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinShark.Infrastructure.Email;

public sealed class NoOpEmailService : IEmailService
{
    private readonly ILogger<NoOpEmailService> _logger;

    public NoOpEmailService(ILogger<NoOpEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        _logger.LogWarning("No SMTP configured. Skipping email to {Email}. Subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
