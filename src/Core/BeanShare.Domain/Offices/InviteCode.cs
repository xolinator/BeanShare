namespace BeanShare.Domain.Offices;

public readonly record struct InviteCode
{
    private const string SafeCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int MinLength = 6;
    private const int MaxLength = 10;
    
    public string Value { get; }
    
    public InviteCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Invite code cannot be empty", nameof(value));
            
        if (value.Length < MinLength || value.Length > MaxLength)
            throw new ArgumentException($"Invite code must be between {MinLength} and {MaxLength} characters", nameof(value));
            
        if (!value.All(c => SafeCharacters.Contains(c)))
            throw new ArgumentException("Invite code contains invalid characters", nameof(value));
            
        Value = value;
    }
    
    public static implicit operator string(InviteCode inviteCode) => inviteCode.Value;
    public static implicit operator InviteCode(string value) => new(value);
    
    public override string ToString() => Value;
}