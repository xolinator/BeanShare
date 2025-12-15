using BeanShare.Application.Abstractions;
using BeanShare.Infrastructure.Communication.Templates;

namespace BeanShare.Infrastructure.Communication.Services;

/// <summary>
/// Implementation of settlement email template service.
/// </summary>
public sealed class SettlementEmailTemplateService : ISettlementEmailTemplateService
{
    /// <inheritdoc />
    public (string HtmlBody, string PlainTextBody) GenerateSettlementEmail(
        string userName,
        string spaceName,
        string billingPeriodName,
        DateTime periodStart,
        DateTime periodEnd,
        decimal amountDue,
        string currency,
        decimal consumptionPercentage,
        string? appUrl = null)
    {
        var htmlBody = SettlementEmailTemplate.GenerateHtml(
            userName,
            spaceName,
            billingPeriodName,
            periodStart,
            periodEnd,
            amountDue,
            currency,
            consumptionPercentage,
            appUrl);

        var plainTextBody = SettlementEmailTemplate.GeneratePlainText(
            userName,
            spaceName,
            billingPeriodName,
            periodStart,
            periodEnd,
            amountDue,
            currency,
            consumptionPercentage);

        return (htmlBody, plainTextBody);
    }
}
