using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Aggregates.Settlement;

public sealed class SettlementLine : Entity
{
    private SettlementLine()
    {
        UserId = default!;
        AmountDue = default!;
    }

    public Guid Id { get; private set; }
    public UserId UserId { get; private set; }
    public decimal TotalCoffeeGrams { get; private set; }
    public decimal? TotalMilkMl { get; private set; }
    public Money AmountDue { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public PaymentConfirmation? Confirmation { get; private set; }

    public bool IsConfirmed => Confirmation is not null;

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

    internal void ConfirmPayment(UserId confirmedBy, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(confirmedBy);
        ArgumentNullException.ThrowIfNull(clock);

        if (IsConfirmed)
        {
            throw new InvalidOperationException("Payment has already been confirmed");
        }

        Confirmation = PaymentConfirmation.Create(confirmedBy, clock.UtcNow);
    }

    protected override object GetId() => Id;
}