namespace BeanShare.Contracts.Spaces;

public sealed record CreateSpaceRequest
{
    public required string Name { get; init; }
}

public sealed record CreateSpaceResponse
{
    public required Guid SpaceId { get; init; }
    public required string InviteCode { get; init; }
    public required string Message { get; init; }
}