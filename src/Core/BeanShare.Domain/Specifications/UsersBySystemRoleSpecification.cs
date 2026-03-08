using System.Linq.Expressions;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;

namespace BeanShare.Domain.Specifications;

public sealed class UsersBySystemRoleSpecification : Spec<User>
{
    private readonly SystemRole _role;

    public UsersBySystemRoleSpecification(SystemRole role)
    {
        _role = role;
    }

    public override Expression<Func<User, bool>> Criteria => user => user.SystemRole == _role;
    public override string? Reason => $"Users with role {_role}";
}
