namespace BeanShare.Application.Abstractions;

public interface IEmailService
{
    Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);

    Task SendEmailWithAttachmentAsync(EmailMessage message, EmailAttachment attachment, CancellationToken cancellationToken = default);

    Task SendEmailWithAttachmentsAsync(EmailMessage message, IEnumerable<EmailAttachment> attachments, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmailSendResult>> SendBatchAsync(IReadOnlyList<EmailMessage> messages, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmailSendResult>> SendBatchAsync(IReadOnlyList<EmailMessage> messages, IReadOnlyList<EmailAttachment> sharedAttachments, CancellationToken cancellationToken = default);
}

public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? PlainTextBody = null
);

public sealed record EmailAttachment(
    string FileName,
    byte[] Content,
    string ContentType
);

public sealed record EmailSendResult(
    string Recipient,
    bool Success,
    string? ErrorMessage = null
);
