using System.Linq.Expressions;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Specifications;

public sealed class SpaceByInviteCodeSpecification : Spec<Space>
{
    private readonly InviteCode _inviteCode;

    public SpaceByInviteCodeSpecification(InviteCode inviteCode)
    {
        _inviteCode = inviteCode;
    }

    public override Expression<Func<Space, bool>> Criteria => space => space.InviteCode == _inviteCode;
    public override string? Reason => $"Space with invite code {_inviteCode}";
}