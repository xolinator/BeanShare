namespace BeanShare.Application.Abstractions;

/// <summary>
/// Service for generating settlement email templates.
/// </summary>
public interface ISettlementEmailTemplateService
{
    /// <summary>
    /// Generates HTML and plain text email bodies for a settlement notification.
    /// </summary>
    (string HtmlBody, string PlainTextBody) GenerateSettlementEmail(
        string userName,
        string spaceName,
        string billingPeriodName,
        DateTime periodStart,
        DateTime periodEnd,
        decimal amountDue,
        string currency,
        decimal consumptionPercentage,
        string? appUrl = null);
}
