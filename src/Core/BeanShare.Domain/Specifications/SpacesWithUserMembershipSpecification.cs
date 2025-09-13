using System.Linq.Expressions;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;

namespace BeanShare.Domain.Specifications;

public sealed class SpacesWithUserMembershipSpecification : Spec<Space>
{
    private readonly UserId _userId;

    public SpacesWithUserMembershipSpecification(UserId userId)
    {
        _userId = userId;
    }

    public override Expression<Func<Space, bool>> Criteria => space => space.Members.Any(m => m.UserId == _userId);
    public override string? Reason => $"Spaces where user {_userId} is a member";
}