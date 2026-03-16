namespace FinShark.Application.Common;

/// <summary>
/// Abstraction for sending emails.
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
