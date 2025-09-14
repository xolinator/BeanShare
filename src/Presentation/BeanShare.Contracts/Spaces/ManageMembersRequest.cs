namespace BeanShare.Contracts.Spaces;

public sealed record PromoteMemberRequest
{
    public required Guid SpaceId { get; init; }
    public required Guid UserId { get; init; }
}

public sealed record DemoteMemberRequest
{
    public required Guid SpaceId { get; init; }
    public required Guid UserId { get; init; }
}

public sealed record RemoveMemberRequest
{
    public required Guid SpaceId { get; init; }
    public required Guid UserId { get; init; }
}

public sealed record MemberActionResponse
{
    public required string Message { get; init; }
    public required Guid SpaceId { get; init; }
    public required Guid UserId { get; init; }
}