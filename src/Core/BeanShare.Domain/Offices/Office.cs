using BeanShare.Domain.Common;

namespace BeanShare.Domain.Offices;

public sealed class Office
{
    private readonly List<OfficeMembership> _members = new();
    
    public OfficeId Id { get; private set; }
    public string Name { get; private set; }
    public InviteCode InviteCode { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<OfficeMembership> Members => _members.AsReadOnly();
    
    public DateTime CreatedAt { get; private set; }
    
    private Office() 
    {
        Name = string.Empty;
    }
    
    internal Office(OfficeId id, string name, InviteCode inviteCode, UserId creatorUserId, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Office name cannot be empty", nameof(name));
            
        Id = id;
        Name = name.Trim();
        InviteCode = inviteCode;
        IsActive = true;
        CreatedAt = createdAt;
        
        var creatorMembership = new OfficeMembership(creatorUserId, OfficeRole.Admin, createdAt);
        _members.Add(creatorMembership);
    }
    
    public bool HasMember(UserId userId) => _members.Any(m => m.UserId == userId);
    
    public OfficeMembership? GetMember(UserId userId) => _members.FirstOrDefault(m => m.UserId == userId);
    
    public bool IsAdmin(UserId userId) => 
        _members.FirstOrDefault(m => m.UserId == userId)?.Role == OfficeRole.Admin;
    
    public int AdminCount => _members.Count(m => m.Role == OfficeRole.Admin);
}