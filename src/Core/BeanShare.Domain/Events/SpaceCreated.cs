using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Events;

public sealed record SpaceCreated(
    SpaceId SpaceId,
    string SpaceName,
    UserId CreatorUserId,
    InviteCode InviteCode,
    DateTime OccurredOn
) : IDomainEvent;