namespace BeanShare.Domain.Common;

public abstract class Entity
{
    protected Entity()
    {
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other || GetType() != other.GetType())
            return false;

        return ReferenceEquals(this, other) || GetId().Equals(other.GetId());
    }

    public override int GetHashCode()
    {
        return GetId().GetHashCode();
    }

    protected abstract object GetId();
}