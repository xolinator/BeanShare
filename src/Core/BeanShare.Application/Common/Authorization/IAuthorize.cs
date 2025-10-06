namespace BeanShare.Application.Common.Authorization;

/// <summary>
/// Marker interface for requests that require authorization.
/// Requests implementing this interface will be processed by AuthorizationBehavior.
/// </summary>
public interface IAuthorize
{
}