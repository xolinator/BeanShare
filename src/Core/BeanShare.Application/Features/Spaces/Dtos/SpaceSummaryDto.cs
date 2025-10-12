using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Spaces.Dtos;
public sealed record SpaceSummaryDto
{
    public required SpaceId Id { get; init; }
    public required string Name { get; init; }
    public required string InviteCode { get; init; }
    public required int MemberCount { get; init; }
    public required DateTime CreatedAt { get; init; }

    // Component compatibility properties
    public Guid SpaceId => Id.Value;
    public string Role { get; init; } = "Member"; // Will be set from query
}
