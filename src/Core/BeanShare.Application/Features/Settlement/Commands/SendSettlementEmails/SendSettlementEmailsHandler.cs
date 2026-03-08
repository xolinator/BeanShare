using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Settlement.Dtos;
using BeanShare.Application.Features.Settlement.Queries.GetSettlementReportData;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using MediatR;

namespace BeanShare.Application.Features.Settlement.Commands.SendSettlementEmails;

/// <summary>
/// Handler for SendSettlementEmailsCommand.
/// </summary>
public sealed class SendSettlementEmailsHandler : IRequestHandler<SendSettlementEmailsCommand, Result<int>>
{
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly ISettlementEmailTemplateService _templateService;

    public SendSettlementEmailsHandler(
        IMediator mediator,
        IEmailService emailService,
        ISettlementEmailTemplateService templateService)
    {
        _mediator = mediator;
        _emailService = emailService;
        _templateService = templateService;
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
            catch
            {
            }
        }

        return Result<int>.Success(emailsSent);
    }
}
