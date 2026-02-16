namespace BeanShare.Contracts.Admin;

public sealed class GetAllUsersRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public int? RoleFilter { get; set; }
    public bool? ActiveFilter { get; set; }
}

public sealed class GetAllUsersResponse
{
    public required IReadOnlyList<AdminUserItem> Users { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public sealed class AdminUserItem
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public string? PictureUrl { get; set; }
    public required string Provider { get; set; }
    public required string SystemRole { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }
    public int SpaceCount { get; set; }
}

public sealed class AdminUserDetailResponse
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public string? PictureUrl { get; set; }
    public required string Provider { get; set; }
    public required string SystemRole { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }
    public string? PreferredCurrencyCode { get; set; }
    public required IReadOnlyList<UserSpaceMembershipItem> Memberships { get; set; }
}

public sealed class UserSpaceMembershipItem
{
    public Guid SpaceId { get; set; }
    public required string SpaceName { get; set; }
    public required string Role { get; set; }
    public DateTime JoinedAt { get; set; }
}

public sealed class SetUserRoleRequest
{
    public required string Role { get; set; }
}
