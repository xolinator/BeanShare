using BeanShare.Application.Abstractions;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;

namespace BeanShare.Api.Infrastructure.Mocks;

public sealed class MockSpaceRepository : ISpaceRepository
{
    private readonly List<Space> _spaces = new();
    private readonly IClock _clock;

    public MockSpaceRepository(IClock clock)
    {
        _clock = clock;
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