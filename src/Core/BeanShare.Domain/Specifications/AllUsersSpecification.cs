using System.Linq.Expressions;
using BeanShare.Domain.Entities;

namespace BeanShare.Domain.Specifications;

public sealed class AllUsersSpecification : Spec<User>
{
    public override Expression<Func<User, bool>> Criteria => user => true;
    public override string? Reason => "All users";
}
