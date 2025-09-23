using BeanShare.Application.Abstractions;

namespace BeanShare.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BeanShareDbContext _context;

    public UnitOfWork(BeanShareDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}