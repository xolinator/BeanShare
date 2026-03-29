using BeanShare.Domain.Common;

namespace BeanShare.Application.Abstractions;

public interface IDomainEventDispatcher
{
    IReadOnlyList<IDomainEvent> CollectDomainEvents();
    Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default);
}
