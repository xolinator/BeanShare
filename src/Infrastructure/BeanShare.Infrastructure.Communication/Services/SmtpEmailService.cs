using BeanShare.Application.Abstractions;
using BeanShare.Infrastructure.Communication.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BeanShare.Infrastructure.Communication.Services;

// SMTP email service using MailKit
// TODO: Add retry logic with Polly for transient failures
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

            var secureSocketOptions = _options.EnableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, secureSocketOptions, cancellationToken);

            if (!string.IsNullOrEmpty(_options.SmtpUsername))
            {
                await client.AuthenticateAsync(_options.SmtpUsername, _options.SmtpPassword, cancellationToken);
            }

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

    private MimeMessage CreateMimeMessage(EmailMessage message, IEnumerable<EmailAttachment> attachments)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        email.To.Add(MailboxAddress.Parse(message.To));
        email.Subject = message.Subject;

        var builder = new BodyBuilder
        {
            HtmlBody = message.HtmlBody
        };

        if (!string.IsNullOrEmpty(message.PlainTextBody))
        {
            builder.TextBody = message.PlainTextBody;
        }

        foreach (var attachment in attachments)
        {
            builder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
        }

        email.Body = builder.ToMessageBody();

        return email;
    }
}
