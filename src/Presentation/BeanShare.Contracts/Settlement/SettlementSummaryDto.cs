namespace BeanShare.Contracts.Settlement;

public record SettlementSummaryDto(
    Guid Id,
    string BillingPeriodName,
    DateTime GeneratedAt,
    decimal TotalAmount,
    string Currency,
    string Status,
    int ConfirmedLinesCount,
    int TotalLinesCount,
    int UserCount,
    decimal YourShare
);
