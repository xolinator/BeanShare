namespace BeanShare.Application.Common;

public readonly record struct Error(string Code, string Message)
{
    public static Error SpaceNotFound(Guid spaceId) => 
        new("SPACE_NOT_FOUND", $"Coffee space with ID {spaceId} was not found");

    public static Error InviteCodeNotFound(string inviteCode) => 
        new("INVITE_CODE_INVALID", $"Invite code '{inviteCode}' is not valid or has expired");

    public static Error AlreadySpaceMember(Guid spaceId, Guid userId) => 
        new("ALREADY_MEMBER", $"User {userId} is already a member of coffee space {spaceId}");

    public static Error InsufficientSpacePrivileges(string action) => 
        new("INSUFFICIENT_PRIVILEGES", $"User lacks privileges to {action} in this coffee space");

    public static Error LastAdminProtection() => 
        new("LAST_ADMIN_PROTECTION", "Cannot remove or demote the last admin of a coffee space");

    public static Error InvalidSpaceName(string? name) => 
        new("INVALID_SPACE_NAME", $"Coffee space name '{name}' is invalid or empty");

    public static Error ValidationFailure(string field, string message) => 
        new("VALIDATION_ERROR", $"{field}: {message}");

    public static Error SystemFailure(string operation) => 
        new("SYSTEM_ERROR", $"System error occurred during {operation}");
}