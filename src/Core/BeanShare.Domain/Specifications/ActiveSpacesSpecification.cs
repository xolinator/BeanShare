using System.Linq.Expressions;
using BeanShare.Domain.Aggregates.Space;

namespace BeanShare.Domain.Specifications;

public sealed class ActiveSpacesSpecification : Spec<Space>
{
    public override Expression<Func<Space, bool>> Criteria => space => space.IsActive;
    public override string? Reason => "Active spaces";
}