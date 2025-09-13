using BeanShare.Domain.Enums;

namespace BeanShare.Application.DTOs;

public sealed record MembershipDto(
    Guid UserId,
    SpaceRole Role,
    DateTime JoinedAt
);