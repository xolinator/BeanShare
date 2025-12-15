namespace BeanShare.Application.Abstractions;

/// <summary>
/// Service for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email message with an attachment.
    /// </summary>
    Task SendEmailWithAttachmentAsync(EmailMessage message, EmailAttachment attachment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email message with multiple attachments.
    /// </summary>
    Task SendEmailWithAttachmentsAsync(EmailMessage message, IEnumerable<EmailAttachment> attachments, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an email message.
/// </summary>
public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? PlainTextBody = null
);

/// <summary>
/// Represents an email attachment.
/// </summary>
public sealed record EmailAttachment(
    string FileName,
    byte[] Content,
    string ContentType
);
