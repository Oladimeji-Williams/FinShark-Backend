namespace FinShark.Infrastructure.Email;

/// <summary>
/// SMTP configuration settings
/// </summary>
public sealed class SmtpSettings
{
    public string? Provider { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public bool UseDefaultCredentials { get; set; } = false;
    public int TimeoutInMilliseconds { get; set; } = 15000;
}
