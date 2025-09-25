using BeanShare.Application.Abstractions;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Repositories;

public sealed class SpaceRepository : ISpaceRepository
{
    private readonly BeanShareDbContext _context;

    public SpaceRepository(BeanShareDbContext context)
    {
        _context = context;
    }

    public async Task<Space?> GetSingleBySpecAsync(ISpec<Space> specification, CancellationToken cancellationToken = default)
    {
        return await _context.Spaces
            .Include(s => s.Members)
            .Where(specification.Criteria)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Space>> GetBySpecAsync(ISpec<Space> specification, CancellationToken cancellationToken = default)
    {
        return await _context.Spaces
            .Include(s => s.Members)
            .Where(specification.Criteria)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Space space, CancellationToken cancellationToken = default)
    {
        await _context.Spaces.AddAsync(space, cancellationToken);
    }

    public Task UpdateAsync(Space space, CancellationToken cancellationToken = default)
    {
        _context.Spaces.Update(space);
        return Task.CompletedTask;
    }
}