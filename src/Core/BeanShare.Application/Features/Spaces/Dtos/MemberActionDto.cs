namespace BeanShare.Application.Features.Spaces.Dtos;

public sealed class MemberActionDto
{
    public required Guid SpaceId { get; init; }
    public required Guid UserId { get; init; }
    public required string Action { get; init; }
    public required DateTime Timestamp { get; init; }
}