namespace BeanShare.Contracts.Settlement;

public record SettlementSummaryDto(
    Guid Id,
    Guid BillingPeriodId,
    string BillingPeriodName,
    DateTime GeneratedAt,
    decimal TotalAmount,
    string Currency,
    string Status,
    int ConfirmedLinesCount,
    int TotalLinesCount,
    int ParticipantCount,
    decimal? UserShare
);
