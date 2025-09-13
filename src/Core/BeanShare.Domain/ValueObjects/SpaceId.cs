namespace BeanShare.Domain.ValueObjects;

public readonly record struct SpaceId(Guid Value)
{
    public static SpaceId New() => new(Guid.NewGuid());
    
    public static implicit operator Guid(SpaceId spaceId) => spaceId.Value;
    public static implicit operator SpaceId(Guid guid) => new(guid);
    
    public override string ToString() => Value.ToString();
}