namespace BeanShare.Domain.Common;

public readonly record struct NotificationId(Guid Value)
{
    public static NotificationId New() => new(Guid.NewGuid());

    public static implicit operator Guid(NotificationId id) => id.Value;
    public static implicit operator NotificationId(Guid guid) => new(guid);

    public override string ToString() => Value.ToString();
}
