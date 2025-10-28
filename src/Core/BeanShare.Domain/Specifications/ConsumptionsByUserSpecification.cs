using System.Linq.Expressions;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Specifications;

/// <summary>
/// Specification to retrieve all consumptions for a specific user across all spaces
/// </summary>
public sealed class ConsumptionsByUserSpecification : ISpec<ConsumptionEntry>
{
    private readonly UserId _userId;
    private readonly DateTime? _startDate;
    private readonly DateTime? _endDate;

    public ConsumptionsByUserSpecification(
        UserId userId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        ArgumentNullException.ThrowIfNull(userId);

        _userId = userId;
        _startDate = startDate;
        _endDate = endDate;
    }

    public Expression<Func<ConsumptionEntry, bool>> Criteria => c =>
        c.UserId == _userId &&
        (_startDate == null || c.ConsumedAt >= _startDate.Value) &&
        (_endDate == null || c.ConsumedAt <= _endDate.Value);

    public string? Reason => $"Consumptions for user {_userId.Value}" +
        (_startDate.HasValue ? $" from {_startDate.Value:yyyy-MM-dd}" : "") +
        (_endDate.HasValue ? $" to {_endDate.Value:yyyy-MM-dd}" : "");
}
