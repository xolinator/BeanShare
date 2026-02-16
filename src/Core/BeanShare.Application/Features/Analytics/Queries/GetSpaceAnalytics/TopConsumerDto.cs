using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Analytics.Queries.GetSpaceAnalytics;
public sealed record TopConsumerDto
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public int CupCount { get; init; }
    public Money? TotalCost { get; init; }
}
