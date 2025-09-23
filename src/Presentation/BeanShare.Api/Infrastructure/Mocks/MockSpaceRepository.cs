using BeanShare.Application.Abstractions;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Api.Infrastructure.Mocks;

public sealed class MockSpaceRepository : ISpaceRepository
{
    private readonly List<Space> _spaces = new();
    private readonly IClock _clock;

    public MockSpaceRepository(IClock clock)
    {
        _clock = clock;
    }

    public Task<Space?> GetByIdAsync(SpaceId spaceId, CancellationToken ct = default)
    {
        var space = _spaces.FirstOrDefault(s => s.Id == spaceId);
        return Task.FromResult(space);
    }

    public Task<Space?> GetByInviteCodeAsync(InviteCode inviteCode, CancellationToken ct = default)
    {
        var space = _spaces.FirstOrDefault(s => s.InviteCode == inviteCode);
        return Task.FromResult(space);
    }

    public Task AddAsync(Space space, CancellationToken ct = default)
    {
        _spaces.Add(space);
        return Task.CompletedTask;
    }

    public void Update(Space space)
    {
        var existingIndex = _spaces.FindIndex(s => s.Id == space.Id);
        if (existingIndex >= 0)
        {
            _spaces[existingIndex] = space;
        }
    }

    public Task<List<Space>> GetBySpecificationAsync(ISpec<Space> specification, CancellationToken ct = default)
    {
        var predicate = specification.Criteria.Compile();
        var result = _spaces.Where(predicate).ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Space>> GetUserSpacesAsync(UserId userId, CancellationToken ct = default)
    {
        var userSpaces = _spaces.Where(s => s.HasMember(userId)).ToList();
        return Task.FromResult<IReadOnlyList<Space>>(userSpaces);
    }

    public Task UpdateAsync(Space space, CancellationToken ct = default)
    {
        Update(space);
        return Task.CompletedTask;
    }
}