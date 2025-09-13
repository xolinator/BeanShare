using BeanShare.Domain.Exceptions;

namespace BeanShare.Domain.ValueObjects;

public readonly record struct InviteCode
{
    private const string SafeCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int MinLength = 6;
    private const int MaxLength = 10;
    
    public string Value { get; }
    
    public InviteCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new SpaceDomainException("Invite code cannot be empty");
            
        if (value.Length < MinLength || value.Length > MaxLength)
            throw new SpaceDomainException($"Invite code must be between {MinLength} and {MaxLength} characters");
            
        if (!value.All(c => SafeCharacters.Contains(c)))
            throw new SpaceDomainException("Invite code contains invalid characters");
            
        Value = value;
    }
    
    public static implicit operator string(InviteCode inviteCode) => inviteCode.Value;
    public static implicit operator InviteCode(string value) => new(value);
    
    public override string ToString() => Value;
}