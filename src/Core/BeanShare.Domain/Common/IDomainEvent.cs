using MediatR;

namespace BeanShare.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}