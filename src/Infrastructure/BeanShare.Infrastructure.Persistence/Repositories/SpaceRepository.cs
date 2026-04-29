using BeanShare.Application.Abstractions;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Repositories;

public sealed class SpaceRepository : ISpaceRepository
{
    private readonly BeanShareDbContext _context;

    public SpaceRepository(BeanShareDbContext context)
    {
        _context = context;
    }

    public async Task<Space?> GetByIdAsync(SpaceId id, CancellationToken cancellationToken = default)
    {
        return await _context.Spaces
            .Include(s => s.Members)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
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
        // TODO: Add pagination support when space count gets large (>100)
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
        // Space is always loaded within the same DbContext scope before UpdateAsync is called,
        // so it is already tracked. EF Core's snapshot change tracking handles all mutations:
        //   - scalar property changes  → UPDATE
        //   - new Members added        → INSERT into SpaceMemberships
        //   - Members removed          → DELETE from SpaceMemberships (OwnsMany orphan deletion)
        return Task.CompletedTask;
    }

    public async Task<int> GetUserSpaceCountAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _context.Spaces
            .CountAsync(s => s.Members.Any(m => m.UserId == userId), cancellationToken);
    }

    public async Task<Dictionary<UserId, int>> GetSpaceCountsForUsersAsync(
        IEnumerable<UserId> userIds, CancellationToken cancellationToken = default)
    {
        var idList = userIds.ToList();
        if (idList.Count == 0)
            return new Dictionary<UserId, int>();

        var counts = await _context.Spaces
            .SelectMany(s => s.Members)
            .Where(m => idList.Contains(m.UserId))
            .GroupBy(m => m.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(x => x.UserId, x => x.Count);
    }
}