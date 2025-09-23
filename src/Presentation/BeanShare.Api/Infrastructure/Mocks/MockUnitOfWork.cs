using BeanShare.Application.Abstractions;

namespace BeanShare.Api.Infrastructure.Mocks;

public sealed class MockUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }
}