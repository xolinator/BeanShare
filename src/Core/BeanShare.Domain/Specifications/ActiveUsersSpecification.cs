using System.Linq.Expressions;
using BeanShare.Domain.Entities;

namespace BeanShare.Domain.Specifications;

public sealed class ActiveUsersSpecification : Spec<User>
{
    public override Expression<Func<User, bool>> Criteria => user => user.IsActive;
    public override string? Reason => "Active users";
}
