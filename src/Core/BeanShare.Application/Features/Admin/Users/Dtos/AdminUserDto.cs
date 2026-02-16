using BeanShare.Domain.Enums;

namespace BeanShare.Application.Features.Admin.Users.Dtos;
public sealed record AdminUserDto(
    Guid Id,
    string Email,
    string Name,
    string? PictureUrl,
    AuthenticationProvider Provider,
    SystemRole SystemRole,
    bool IsActive,
    DateTime CreatedAt,
    DateTime LastLoginAt,
    DateTime? DeactivatedAt,
    int SpaceCount);
public sealed record AdminUserDetailDto(
    Guid Id,
    string Email,
    string Name,
    string? PictureUrl,
    AuthenticationProvider Provider,
    SystemRole SystemRole,
    bool IsActive,
    DateTime CreatedAt,
    DateTime LastLoginAt,
    DateTime? DeactivatedAt,
    string? PreferredCurrencyCode,
    IReadOnlyList<UserSpaceMembershipDto> Memberships);
public sealed record UserSpaceMembershipDto(
    Guid SpaceId,
    string SpaceName,
    SpaceRole Role,
    DateTime JoinedAt);
