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
        SeedData();
    }

    private void SeedData()
    {
        var spaceId = new SpaceId(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var userId = new UserId(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var inviteCode = new InviteCode("ABCD2345");

        var space = Space.Create(spaceId, "Office Coffee Group", Currency.USD, userId, inviteCode, _clock);

        _spaces.Add(space);
    }

    public Task<Space?> GetByIdAsync(SpaceId id, CancellationToken ct = default)
    {
        var space = _spaces.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(space);
    }

    public Task<Space?> GetSingleBySpecAsync(ISpec<Space> specification, CancellationToken ct = default)
    {
        var predicate = specification.Criteria.Compile();
        var space = _spaces.FirstOrDefault(predicate);
        return Task.FromResult(space);
    }

    public Task<IReadOnlyList<Space>> GetBySpecAsync(ISpec<Space> specification, CancellationToken ct = default)
    {
        var predicate = specification.Criteria.Compile();
        var result = _spaces.Where(predicate).ToList();
        return Task.FromResult<IReadOnlyList<Space>>(result);
    }

    public Task AddAsync(Space space, CancellationToken ct = default)
    {
        _spaces.Add(space);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Space space, CancellationToken ct = default)
    {
        var existingIndex = _spaces.FindIndex(s => s.Id == space.Id);
        if (existingIndex >= 0)
        {
            _spaces[existingIndex] = space;
        }
        return Task.CompletedTask;
    }
}