using BeanShare.Application.Abstractions;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
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

    public async Task<Space?> GetByInviteCodeAsync(InviteCode code, CancellationToken cancellationToken = default)
    {
        return await _context.Spaces
            .Include(s => s.Members)
            .FirstOrDefaultAsync(s => s.InviteCode == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Space>> GetUserSpacesAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _context.Spaces
            .Include(s => s.Members)
            .Where(s => s.Members.Any(m => m.UserId == userId))
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