namespace BeanShare.Application.Features.Spaces.Dtos;

public sealed record SpaceDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string CurrencyCode { get; init; }
    public required string InviteCode { get; init; }
    public required bool IsActive { get; init; }
    public required Guid CreatedBy { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required int MemberCount { get; init; }
    public required IReadOnlyList<MembershipDto> Members { get; init; }
}