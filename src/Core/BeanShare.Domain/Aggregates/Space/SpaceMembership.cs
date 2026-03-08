using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;

namespace BeanShare.Domain.Aggregates.Space;

public sealed class SpaceMembership
{
    public UserId UserId { get; private set; }
    public SpaceRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }

    private SpaceMembership(UserId userId, SpaceRole role, DateTime joinedAt)
    {
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }

    public static SpaceMembership Create(UserId userId, SpaceRole role, DateTime joinedAt)
    {
        return new SpaceMembership(userId, role, joinedAt);
    }

    internal void ChangeRole(SpaceRole newRole)
    {
        Role = newRole;
    }

    public bool IsAdmin => Role == SpaceRole.Admin;
    public bool IsMember => Role == SpaceRole.Member;

    public override bool Equals(object? obj) =>
        obj is SpaceMembership other && UserId.Equals(other.UserId);

    public override int GetHashCode() => UserId.GetHashCode();
}
