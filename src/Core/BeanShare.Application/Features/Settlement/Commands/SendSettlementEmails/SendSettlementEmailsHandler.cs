using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Settlement.Queries.GetSettlementReportData;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BeanShare.Application.Features.Settlement.Commands.SendSettlementEmails;
public sealed class SendSettlementEmailsHandler : IRequestHandler<SendSettlementEmailsCommand, Result<int>>
{
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly ISettlementEmailTemplateService _templateService;
    private readonly ISettlementPdfGenerator _pdfGenerator;
    private readonly ILogger<SendSettlementEmailsHandler> _logger;

    public SendSettlementEmailsHandler(
        IMediator mediator,
        IEmailService emailService,
        ISettlementEmailTemplateService templateService,
        ISettlementPdfGenerator pdfGenerator,
        ILogger<SendSettlementEmailsHandler> logger)
    {
        _mediator = mediator;
        _emailService = emailService;
        _templateService = templateService;
        _pdfGenerator = pdfGenerator;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(SendSettlementEmailsCommand command, CancellationToken cancellationToken)
    {
        var reportDataResult = await _mediator.Send(new GetSettlementReportDataQuery(command.SettlementId), cancellationToken);

        if (!reportDataResult.IsSuccess)
            return Result<int>.Failure(reportDataResult.Errors.ToArray());

        var reportData = reportDataResult.Value;
        var messages = new List<EmailMessage>();

        foreach (var line in reportData.Lines)
        {
            if (string.IsNullOrEmpty(line.UserEmail))
                continue;

            var (htmlBody, plainTextBody) = _templateService.GenerateSettlementEmail(
                line.UserName,
                reportData.SpaceName,
                reportData.BillingPeriodName,
                reportData.PeriodStart,
                reportData.PeriodEnd,
                line.AmountDue,
                reportData.Currency,
                line.ConsumptionPercentage);

            messages.Add(new EmailMessage(
                line.UserEmail,
                $"Settlement Ready - {reportData.SpaceName}",
                htmlBody,
                plainTextBody));
        }

        if (messages.Count == 0)
            return Result<int>.Success(0);

        var attachments = new List<EmailAttachment>();
        if (command.AttachPdf)
        {
            try
            {
                var pdfBytes = _pdfGenerator.Generate(reportData);
                var fileName = $"Settlement_{reportData.BillingPeriodName.Replace(" ", "_")}_{reportData.GeneratedAt:yyyyMMdd}.pdf";
                attachments.Add(new EmailAttachment(fileName, pdfBytes, "application/pdf"));
                _logger.LogInformation("Generated {Size}KB PDF attachment for settlement emails", pdfBytes.Length / 1024);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate PDF attachment, sending emails without it");
            }
        }

        var results = await _emailService.SendBatchAsync(messages, attachments, cancellationToken);

        var sent = results.Count(r => r.Success);
        var failed = results.Where(r => !r.Success).ToList();

        foreach (var failure in failed)
            _logger.LogError("Settlement email failed for {Recipient}: {Error}", failure.Recipient, failure.ErrorMessage);

        if (sent == 0 && failed.Count > 0)
        {
            var failedRecipients = string.Join(", ", failed.Select(f => f.Recipient));
            return Result<int>.Failure(Error.DomainError($"All emails failed. Recipients: {failedRecipients}"));
        }

        _logger.LogInformation("Settlement emails: {Sent} sent, {Failed} failed out of {Total} (PDF attached: {PdfAttached})",
            sent, failed.Count, messages.Count, attachments.Count > 0);

        return Result<int>.Success(sent);
    }
}
