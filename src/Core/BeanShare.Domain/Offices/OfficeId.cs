namespace BeanShare.Domain.Offices;

public readonly record struct OfficeId(Guid Value)
{
    public static OfficeId New() => new(Guid.NewGuid());
    
    public static implicit operator Guid(OfficeId officeId) => officeId.Value;
    public static implicit operator OfficeId(Guid guid) => new(guid);
    
    public override string ToString() => Value.ToString();
}