using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using BeanShare.Domain.Enums;

namespace BeanShare.Domain.Events;

public sealed record SpaceMembershipChanged(
    SpaceId SpaceId,
    UserId UserId,
    MembershipChangeType ChangeType,
    SpaceRole? NewRole,
    DateTime OccurredOn
) : IDomainEvent;