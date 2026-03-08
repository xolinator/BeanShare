using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Settlement.Dtos;
using BeanShare.Application.Features.Settlement.Queries.GetSettlementReportData;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BeanShare.Application.Features.Settlement.Commands.SendSettlementEmails;
public sealed class SendSettlementEmailsHandler : IRequestHandler<SendSettlementEmailsCommand, Result<int>>
{
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly ISettlementEmailTemplateService _templateService;
    private readonly ILogger<SendSettlementEmailsHandler> _logger;

    public SendSettlementEmailsHandler(
        IMediator mediator,
        IEmailService emailService,
        ISettlementEmailTemplateService templateService,
        ILogger<SendSettlementEmailsHandler> logger)
    {
        _mediator = mediator;
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(SendSettlementEmailsCommand command, CancellationToken cancellationToken)
    {
        var reportDataResult = await _mediator.Send(new GetSettlementReportDataQuery(command.SettlementId), cancellationToken);

        if (!reportDataResult.IsSuccess)
        {
            return Result<int>.Failure(reportDataResult.Errors.ToArray());
        }

        var reportData = reportDataResult.Value;
        var emailsSent = 0;

        byte[]? pdfBytes = null;
        if (command.AttachPdf)
        {
            // TODO: PDF attachment not yet implemented - requires ISettlementReportGenerator abstraction in Application layer
            _logger.LogWarning("PDF attachment requested but not yet implemented for settlement emails");
        }

        foreach (var line in reportData.Lines)
        {
            if (string.IsNullOrEmpty(line.UserEmail))
            {
                continue;
            }

            try
            {
                var (htmlBody, plainTextBody) = _templateService.GenerateSettlementEmail(
                    line.UserName,
                    reportData.SpaceName,
                    reportData.BillingPeriodName,
                    reportData.PeriodStart,
                    reportData.PeriodEnd,
                    line.AmountDue,
                    reportData.Currency,
                    line.ConsumptionPercentage);

                var message = new EmailMessage(
                    line.UserEmail,
                    $"Settlement Ready - {reportData.SpaceName}",
                    htmlBody,
                    plainTextBody);

                if (pdfBytes != null)
                {
                    var attachment = new EmailAttachment(
                        $"Settlement_{reportData.BillingPeriodName}.pdf",
                        pdfBytes,
                        "application/pdf");

                    await _emailService.SendEmailWithAttachmentAsync(message, attachment, cancellationToken);
                }
                else
                {
                    await _emailService.SendEmailAsync(message, cancellationToken);
                }

                emailsSent++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send settlement email to {Email}", line.UserEmail);
            }
        }

        return Result<int>.Success(emailsSent);
    }
}
