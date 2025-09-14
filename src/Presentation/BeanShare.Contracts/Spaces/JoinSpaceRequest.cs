namespace BeanShare.Contracts.Spaces;

public sealed record JoinSpaceRequest
{
    public required string InviteCode { get; init; }
}

public sealed record JoinSpaceResponse
{
    public required Guid SpaceId { get; init; }
    public required string SpaceName { get; init; }
    public required string Message { get; init; }
}