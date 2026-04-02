using FinShark.Application.Common;
using MediatorFlow.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace FinShark.Application.Auth.Commands.TestSmtp;

public sealed class TestSmtpCommandHandler(
    IEmailService emailService,
    ILogger<TestSmtpCommandHandler> logger) : IRequestHandler<TestSmtpCommand, string>
{
    public async Task<string> Handle(TestSmtpCommand request, CancellationToken cancellationToken)
    {
        var to = "test@example.com";
        var subject = "SMTP Test";
        var body = "<p>SMTP is working</p>";

        try
        {
            await emailService.SendEmailAsync(to, subject, body);
            return "SMTP test email sent";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SMTP test failed");
            throw;
        }
    }
}
