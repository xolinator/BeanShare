using System.Linq.Expressions;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Specifications;

public sealed class SpaceByIdSpecification : Spec<Space>
{
    private readonly SpaceId _spaceId;

    public SpaceByIdSpecification(SpaceId spaceId)
    {
        _spaceId = spaceId;
    }

    public override Expression<Func<Space, bool>> Criteria => space => space.Id == _spaceId;
    public override string? Reason => $"Space with ID {_spaceId}";
}