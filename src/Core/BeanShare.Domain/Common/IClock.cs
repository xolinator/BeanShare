namespace BeanShare.Domain.Common;

public interface IClock
{
    DateTime UtcNow { get; }
}