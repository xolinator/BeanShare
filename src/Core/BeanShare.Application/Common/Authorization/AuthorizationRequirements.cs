namespace BeanShare.Application.Common.Authorization;

/// <summary>
/// Requires the user to be a member of the specified space.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RequireSpaceMemberAttribute : Attribute
{
    public string SpaceIdPropertyName { get; }

    public RequireSpaceMemberAttribute(string spaceIdPropertyName)
    {
        SpaceIdPropertyName = spaceIdPropertyName;
    }
}

/// <summary>
/// Requires the user to be an admin of the specified space.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RequireSpaceAdminAttribute : Attribute
{
    public string SpaceIdPropertyName { get; }

    public RequireSpaceAdminAttribute(string spaceIdPropertyName)
    {
        SpaceIdPropertyName = spaceIdPropertyName;
    }
}