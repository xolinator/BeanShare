using System.Linq.Expressions;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;

namespace BeanShare.Domain.Specifications;

public sealed class SpacesWhereUserIsAdminSpecification : Spec<Space>
{
    private readonly UserId _userId;

    public SpacesWhereUserIsAdminSpecification(UserId userId)
    {
        _userId = userId;
    }

    public override Expression<Func<Space, bool>> Criteria => space => space.Members.Any(m => m.UserId == _userId && m.Role == SpaceRole.Admin);
    public override string? Reason => $"Spaces where user {_userId} is an admin";
}