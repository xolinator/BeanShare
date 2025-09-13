namespace BeanShare.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}