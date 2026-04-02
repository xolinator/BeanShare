using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;
using BeanShare.Domain.Events;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Aggregates.BillingPeriod;

public sealed class BillingPeriod : AggregateRoot
{
    private BillingPeriod() { }

    public BillingPeriodId Id { get; private set; }
    public SpaceId SpaceId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public BillingState State { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public DateTime? SettledAt { get; private set; }
    public UserId CreatedBy { get; private set; }
    public UserId? ClosedBy { get; private set; }
    public UserId? SettledBy { get; private set; }

    public static BillingPeriod Create(
        SpaceId spaceId,
        string name,
        DateTime startDate,
        DateTime endDate,
        UserId createdBy,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(spaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(createdBy);
        ArgumentNullException.ThrowIfNull(clock);

        if (endDate <= startDate)
        {
            throw new ArgumentException("End date must be after start date");
        }

        if (startDate.Kind != DateTimeKind.Utc || endDate.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Dates must be in UTC");
        }

        var billingPeriod = new BillingPeriod
        {
            Id = new BillingPeriodId(Guid.NewGuid()),
            SpaceId = spaceId,
            Name = name,
            StartDate = startDate.Date,
            EndDate = endDate.Date.AddDays(1).AddTicks(-1),
            State = BillingState.Draft,
            CreatedAt = clock.UtcNow,
            CreatedBy = createdBy
        };

        billingPeriod.RaiseDomainEvent(new BillingPeriodCreated(
            billingPeriod.Id,
            spaceId,
            name,
            startDate,
            endDate,
            createdBy,
            clock.UtcNow
        ));

        return billingPeriod;
    }

    public void Open(UserId userId, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(clock);

        if (State != BillingState.Draft)
        {
            throw new InvalidOperationException($"Cannot open billing period in state {State}");
        }

        State = BillingState.Open;

        RaiseDomainEvent(new BillingPeriodOpened(
            Id,
            SpaceId,
            userId,
            clock.UtcNow
        ));
    }

    public void Close(UserId userId, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(clock);

        if (State != BillingState.Open)
        {
            throw new InvalidOperationException($"Cannot close billing period in state {State}");
        }

        State = BillingState.Closed;
        ClosedAt = clock.UtcNow;
        ClosedBy = userId;

        RaiseDomainEvent(new BillingPeriodClosed(
            Id,
            SpaceId,
            userId,
            clock.UtcNow
        ));
    }

    public void MarkAsSettled(UserId userId, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(clock);

        if (State != BillingState.Closed)
        {
            throw new InvalidOperationException($"Cannot settle billing period in state {State}");
        }

        State = BillingState.Settled;
        SettledAt = clock.UtcNow;
        SettledBy = userId;

        RaiseDomainEvent(new BillingPeriodSettled(
            Id,
            SpaceId,
            userId,
            clock.UtcNow
        ));
    }

    public bool ContainsDate(DateTime date)
    {
        // Use the date as-is - callers should provide UTC dates consistent with StartDate/EndDate
        return date >= StartDate && date <= EndDate;
    }

    public bool OverlapsWith(DateTime startDate, DateTime endDate)
    {
        return startDate < EndDate && endDate > StartDate;
    }

    public bool CanBeOpened()
    {
        return State == BillingState.Draft;
    }

    public bool CanBeClosed()
    {
        return State == BillingState.Open;
    }

    public bool CanBeSettled()
    {
        return State == BillingState.Closed;
    }
}