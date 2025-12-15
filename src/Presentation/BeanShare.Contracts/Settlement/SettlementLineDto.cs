namespace BeanShare.Contracts.Settlement;

public record SettlementLineDto(
    Guid UserId,
    string UserName,
    string UserEmail,
    decimal TotalCoffeeGrams,
    decimal? TotalMilkMl,
    decimal AmountDue,
    decimal ConsumptionPercentage,
    bool IsConfirmed,
    Guid? ConfirmedBy,
    DateTime? ConfirmedAt
);
