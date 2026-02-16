namespace BeanShare.Application.Features.Consumption.Dtos;
public sealed record ConsumptionHistoryItemDto(
    Guid Id,
    Guid SpaceId,
    string SpaceName,
    string ProductName,
    string ProductBrand,
    string ProductType,
    decimal QuantityGrams,
    DateTime ConsumedAt,
    decimal? EstimatedCost,
    string? Currency,
    Guid? BillingPeriodId,
    string? BillingPeriodName
);
