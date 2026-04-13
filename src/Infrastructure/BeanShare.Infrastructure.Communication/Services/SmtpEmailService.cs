using BeanShare.Application.Abstractions;
using BeanShare.Infrastructure.Communication.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BeanShare.Infrastructure.Communication.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailOptions> options, ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        await SendEmailWithAttachmentsAsync(message, [], cancellationToken);
    }

    public async Task SendEmailWithAttachmentAsync(EmailMessage message, EmailAttachment attachment, CancellationToken cancellationToken = default)
    {
        await SendEmailWithAttachmentsAsync(message, [attachment], cancellationToken);
    }

    public async Task SendEmailWithAttachmentsAsync(EmailMessage message, IEnumerable<EmailAttachment> attachments, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Email sending disabled. Would send to {To}: {Subject}", message.To, message.Subject);
            return;
        }

        var email = CreateMimeMessage(message, attachments);

        try
        {
            using var client = new SmtpClient();
            await ConnectAndAuthenticateAsync(client, cancellationToken);
            await client.SendAsync(email, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To}: {Subject}", message.To, message.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}: {Subject}", message.To, message.Subject);
            throw;
        }
    }

    public Task<IReadOnlyList<EmailSendResult>> SendBatchAsync(
        IReadOnlyList<EmailMessage> messages,
        CancellationToken cancellationToken = default)
        => SendBatchAsync(messages, [], cancellationToken);

    public async Task<IReadOnlyList<EmailSendResult>> SendBatchAsync(
        IReadOnlyList<EmailMessage> messages,
        IReadOnlyList<EmailAttachment> sharedAttachments,
        CancellationToken cancellationToken = default)
    {
        if (messages.Count == 0)
            return [];

        if (!_options.Enabled)
        {
            _logger.LogInformation("Email sending disabled. Would send batch of {Count} emails", messages.Count);
            return messages.Select(m => new EmailSendResult(m.To, true)).ToList();
        }

        var results = new List<EmailSendResult>(messages.Count);

        try
        {
            using var client = new SmtpClient();
            await ConnectAndAuthenticateAsync(client, cancellationToken);

            foreach (var message in messages)
            {
                try
                {
                    var mime = CreateMimeMessage(message, sharedAttachments);
                    await client.SendAsync(mime, cancellationToken);
                    results.Add(new EmailSendResult(message.To, true));
                    _logger.LogInformation("Batch email sent to {To}", message.To);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Batch email failed for {To}", message.To);
                    results.Add(new EmailSendResult(message.To, false, ex.Message));
                }
            }

            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP connection failed during batch send");
            foreach (var unsent in messages.Skip(results.Count))
                results.Add(new EmailSendResult(unsent.To, false, "SMTP connection lost"));
        }

        return results;
    }

    private async Task ConnectAndAuthenticateAsync(SmtpClient client, CancellationToken cancellationToken)
    {
        var secureSocketOptions = _options.EnableSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, secureSocketOptions, cancellationToken);

        if (!string.IsNullOrEmpty(_options.SmtpUsername))
            await client.AuthenticateAsync(_options.SmtpUsername, _options.SmtpPassword, cancellationToken);
    }

    private MimeMessage CreateMimeMessage(EmailMessage message, IEnumerable<EmailAttachment> attachments)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        email.To.Add(MailboxAddress.Parse(message.To));
        email.Subject = message.Subject;

        var builder = new BodyBuilder { HtmlBody = message.HtmlBody };

        if (!string.IsNullOrEmpty(message.PlainTextBody))
            builder.TextBody = message.PlainTextBody;

        foreach (var attachment in attachments)
            builder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));

        email.Body = builder.ToMessageBody();
        return email;
    }
}
