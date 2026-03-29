using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;
using MediatR;

namespace BeanShare.Infrastructure.Persistence;

public sealed class DomainEventDispatcher(
    BeanShareDbContext context,
    IMediator mediator) : IDomainEventDispatcher
{
    public IReadOnlyList<IDomainEvent> CollectDomainEvents()
        => context.CollectDomainEvents();

    public async Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events)
            await mediator.Publish(domainEvent, cancellationToken);
    }
}
