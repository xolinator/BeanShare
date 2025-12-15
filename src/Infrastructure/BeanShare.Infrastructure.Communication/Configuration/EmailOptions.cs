namespace BeanShare.Infrastructure.Communication.Configuration;

/// <summary>
/// Configuration options for email service.
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>
    /// SMTP server hostname.
    /// </summary>
    public string SmtpHost { get; set; } = "smtp.gmail.com";

    /// <summary>
    /// SMTP server port.
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// SMTP username for authentication.
    /// </summary>
    public string SmtpUsername { get; set; } = "";

    /// <summary>
    /// SMTP password for authentication.
    /// </summary>
    public string SmtpPassword { get; set; } = "";

    /// <summary>
    /// Sender email address.
    /// </summary>
    public string FromAddress { get; set; } = "noreply@beanshare.app";

    /// <summary>
    /// Sender display name.
    /// </summary>
    public string FromName { get; set; } = "BeanShare";

    /// <summary>
    /// Enable SSL/TLS for SMTP connection.
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Enable email sending. When false, emails are logged but not sent.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
