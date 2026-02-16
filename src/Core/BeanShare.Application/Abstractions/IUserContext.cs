using BeanShare.Domain.Common;

namespace BeanShare.Application.Abstractions;

/// <summary>
/// Provides access to the currently authenticated user's identity and claims.
/// </summary>
public interface IUserContext
{
    UserId CurrentUserId { get; }
    string Email { get; }
    IReadOnlyCollection<string> Roles { get; }
}
