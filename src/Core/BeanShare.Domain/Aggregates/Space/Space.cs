using BeanShare.Domain.Common;
using BeanShare.Domain.Events;
using BeanShare.Domain.ValueObjects;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
using BeanShare.Domain.Exceptions;

namespace BeanShare.Domain.Aggregates.Space;

public sealed class Space : AggregateRoot
{
    private readonly List<SpaceMembership> _members = [];
    
    public SpaceId Id { get; private set; }
    public string Name { get; private set; }
    public InviteCode InviteCode { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<SpaceMembership> Members => _members.AsReadOnly();
    
    public DateTime CreatedAt { get; private set; }
    
    private Space() 
    {
        Name = string.Empty;
    }
    
    internal Space(SpaceId id, string name, InviteCode inviteCode, UserId creatorUserId, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new SpaceDomainException("Space name cannot be empty");
            
        Id = id;
        Name = name.Trim();
        InviteCode = inviteCode;
        IsActive = true;
        CreatedAt = createdAt;
        
        var creatorMembership = new SpaceMembership(creatorUserId, SpaceRole.Admin, createdAt);
        _members.Add(creatorMembership);
    }
    
    public bool HasMember(UserId userId) => _members.Any(m => m.UserId == userId);
    
    public SpaceMembership? GetMember(UserId userId) => _members.FirstOrDefault(m => m.UserId == userId);
    
    public bool IsAdmin(UserId userId) => 
        _members.FirstOrDefault(m => m.UserId == userId)?.Role == SpaceRole.Admin;
    
    public int AdminCount => _members.Count(m => m.Role == SpaceRole.Admin);
    
    public static Space Create(
        SpaceId id,
        string name,
        UserId creatorUserId,
        InviteCode inviteCode,
        IClock clock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new SpaceDomainException("Space name cannot be empty");
            
        var createdAt = clock.UtcNow;
        
        var space = new Space(id, name, inviteCode, creatorUserId, createdAt);
        
        space.RaiseDomainEvent(new SpaceCreated(
            id,
            name.Trim(),
            creatorUserId,
            inviteCode,
            createdAt
        ));
        
        return space;
    }
    
    public void Join(UserId userId, IClock clock)
    {
        if (HasMember(userId))
            return;
            
        var joinedAt = clock.UtcNow;
        var membership = new SpaceMembership(userId, SpaceRole.Member, joinedAt);
        _members.Add(membership);
        
        RaiseDomainEvent(new SpaceMembershipChanged(
            Id,
            userId,
            MembershipChangeType.MemberJoined,
            SpaceRole.Member,
            joinedAt
        ));
    }
    
    public void PromoteMember(UserId userId, IClock clock)
    {
        var member = GetMember(userId);
        if (member == null)
            throw new SpaceDomainException("Cannot promote user who is not a member");
            
        if (member.Role == SpaceRole.Admin)
            return;
            
        member.ChangeRole(SpaceRole.Admin);
        
        RaiseDomainEvent(new SpaceMembershipChanged(
            Id,
            userId,
            MembershipChangeType.MemberPromoted,
            SpaceRole.Admin,
            clock.UtcNow
        ));
    }
    
    public void DemoteMember(UserId userId, IClock clock)
    {
        var member = GetMember(userId);
        if (member == null)
            throw new SpaceDomainException("Cannot demote user who is not a member");
            
        if (member.Role == SpaceRole.Member)
            return;
            
        if (AdminCount <= 1)
            throw new InvariantViolationException("Cannot demote the last admin");
            
        member.ChangeRole(SpaceRole.Member);
        
        RaiseDomainEvent(new SpaceMembershipChanged(
            Id,
            userId,
            MembershipChangeType.MemberDemoted,
            SpaceRole.Member,
            clock.UtcNow
        ));
    }
    
    public void RemoveMember(UserId userId, IClock clock)
    {
        var member = GetMember(userId);
        if (member == null)
            throw new SpaceDomainException("Cannot remove user who is not a member");
            
        if (member.Role == SpaceRole.Admin && AdminCount <= 1)
            throw new InvariantViolationException("Cannot remove the last admin");
            
        _members.Remove(member);
        
        RaiseDomainEvent(new SpaceMembershipChanged(
            Id,
            userId,
            MembershipChangeType.MemberRemoved,
            null,
            clock.UtcNow
        ));
    }
}