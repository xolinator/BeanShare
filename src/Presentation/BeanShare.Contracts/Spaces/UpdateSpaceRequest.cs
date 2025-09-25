namespace BeanShare.Contracts.Spaces;

public sealed record UpdateSpaceRequest
{
    public required Guid SpaceId { get; init; }
    public required string Name { get; init; }
}

public sealed record DeactivateSpaceRequest
{
    public required Guid SpaceId { get; init; }
}

public sealed record RegenerateInviteCodeRequest
{
    public required Guid SpaceId { get; init; }
}

public sealed record SpaceActionResponse
{
    public required Guid SpaceId { get; init; }
    public required string Name { get; init; }
    public required bool IsActive { get; init; }
    public required string Message { get; init; }
}

public sealed record InviteCodeResponse
{
    public required Guid SpaceId { get; init; }
    public required string InviteCode { get; init; }
    public required string Message { get; init; }
}