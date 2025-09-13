using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;

namespace BeanShare.Domain.Entities;

public sealed class SpaceMembership
{
    public UserId UserId { get; private set; }
    public SpaceRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    
    internal SpaceMembership(UserId userId, SpaceRole role, DateTime joinedAt)
    {
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }
    
    internal void ChangeRole(SpaceRole newRole)
    {
        Role = newRole;
    }
    
    public override bool Equals(object? obj) => 
        obj is SpaceMembership other && UserId.Equals(other.UserId);
        
    public override int GetHashCode() => UserId.GetHashCode();
}