using BeanShare.Domain.Common;
using BeanShare.Domain.Events;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Aggregates.Settlement;

public sealed class Settlement : AggregateRoot
{
    private readonly List<SettlementLine> _lines = new();

    private Settlement() { }

    public SettlementId Id { get; private set; }
    public SpaceId SpaceId { get; private set; }
    public BillingPeriodId BillingPeriodId { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public UserId GeneratedBy { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public IReadOnlyCollection<SettlementLine> Lines => _lines.AsReadOnly();

    public static Settlement Create(
        SpaceId spaceId,
        BillingPeriodId billingPeriodId,
        string currency,
        UserId generatedBy,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(spaceId);
        ArgumentNullException.ThrowIfNull(billingPeriodId);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        ArgumentNullException.ThrowIfNull(generatedBy);
        ArgumentNullException.ThrowIfNull(clock);

        var settlement = new Settlement
        {
            Id = new SettlementId(Guid.NewGuid()),
            SpaceId = spaceId,
            BillingPeriodId = billingPeriodId,
            Currency = currency,
            GeneratedBy = generatedBy,
            GeneratedAt = clock.UtcNow,
            TotalAmount = 0
        };

        settlement.RaiseDomainEvent(new SettlementGenerated(
            settlement.Id,
            spaceId,
            billingPeriodId,
            generatedBy,
            clock.UtcNow
        ));

        return settlement;
    }

    public void AddLine(
        UserId userId,
        decimal totalCoffeeGrams,
        decimal amountDue,
        IClock clock,
        decimal? totalMilkMl = null)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(clock);

        if (totalCoffeeGrams < 0)
        {
            throw new ArgumentException("Total coffee grams cannot be negative", nameof(totalCoffeeGrams));
        }

        if (amountDue < 0)
        {
            throw new ArgumentException("Amount due cannot be negative", nameof(amountDue));
        }

        if (totalMilkMl.HasValue && totalMilkMl.Value < 0)
        {
            throw new ArgumentException("Total milk ml cannot be negative", nameof(totalMilkMl));
        }

        if (_lines.Any(l => l.UserId == userId))
        {
            throw new InvalidOperationException($"Settlement line for user {userId} already exists");
        }

        var line = SettlementLine.Create(
            userId,
            totalCoffeeGrams,
            totalMilkMl,
            Money.Create(amountDue, Currency),
            clock
        );

        _lines.Add(line);
        TotalAmount += amountDue;
    }

    public SettlementLine? GetLineForUser(UserId userId)
    {
        return _lines.FirstOrDefault(l => l.UserId == userId);
    }

    public decimal GetAmountDueForUser(UserId userId)
    {
        var line = GetLineForUser(userId);
        return line?.AmountDue.Amount ?? 0;
    }

    public bool HasLineForUser(UserId userId)
    {
        return _lines.Any(l => l.UserId == userId);
    }
}