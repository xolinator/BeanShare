namespace BeanShare.Application.Features.Settlement.Dtos;

public sealed record SettlementSummaryDto(
    Guid Id,
    string BillingPeriodName,
    DateTime GeneratedAt,
    decimal TotalAmount,
    string Currency,
    int UserCount,
    decimal YourShare
);
