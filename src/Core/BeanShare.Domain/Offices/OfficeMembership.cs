using BeanShare.Domain.Common;

namespace BeanShare.Domain.Offices;

public sealed class OfficeMembership
{
    public UserId UserId { get; private set; }
    public OfficeRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    
    internal OfficeMembership(UserId userId, OfficeRole role, DateTime joinedAt)
    {
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }
    
    internal void ChangeRole(OfficeRole newRole)
    {
        Role = newRole;
    }
    
    public override bool Equals(object? obj) => 
        obj is OfficeMembership other && UserId.Equals(other.UserId);
        
    public override int GetHashCode() => UserId.GetHashCode();
}