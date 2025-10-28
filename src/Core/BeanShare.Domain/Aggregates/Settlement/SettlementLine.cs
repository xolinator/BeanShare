using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Aggregates.Settlement;

public sealed class SettlementLine : Entity
{
    private SettlementLine() { }

    public Guid Id { get; private set; }
    public UserId UserId { get; private set; }
    public decimal TotalCoffeeGrams { get; private set; }
    public decimal? TotalMilkMl { get; private set; }
    public Money AmountDue { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static SettlementLine Create(
        UserId userId,
        decimal totalCoffeeGrams,
        decimal? totalMilkMl,
        Money amountDue,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(amountDue);
        ArgumentNullException.ThrowIfNull(clock);

        return new SettlementLine
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TotalCoffeeGrams = totalCoffeeGrams,
            TotalMilkMl = totalMilkMl,
            AmountDue = amountDue,
            CreatedAt = clock.UtcNow
        };
    }

    public decimal GetConsumptionPercentage(decimal totalSpaceConsumption)
    {
        if (totalSpaceConsumption <= 0)
            return 0;

        return (TotalCoffeeGrams / totalSpaceConsumption) * 100;
    }

    protected override object GetId() => Id;
}