using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Spaces.Dtos;

public sealed record SpaceDto
{
    public required SpaceId Id { get; init; }
    public required string Name { get; init; }
    public required string InviteCode { get; init; }
    public required UserId CreatedBy { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required int MemberCount { get; init; }
    public required IReadOnlyList<MembershipDto> Members { get; init; }
}

public sealed record SpaceSummaryDto
{
    public required SpaceId Id { get; init; }
    public required string Name { get; init; }
    public required string InviteCode { get; init; }
    public required int MemberCount { get; init; }
    public required DateTime CreatedAt { get; init; }
}

public sealed record MembershipDto
{
    public required UserId UserId { get; init; }
    public required string Email { get; init; }
    public required string Role { get; init; }
    public required DateTime JoinedAt { get; init; }
}