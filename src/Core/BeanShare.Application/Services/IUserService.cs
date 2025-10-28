using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;

namespace BeanShare.Application.Services;

/// <summary>
/// Service for retrieving user information
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get a user by their ID
    /// </summary>
    Task<User?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get multiple users by their IDs
    /// </summary>
    Task<IReadOnlyList<User>> GetByIdsAsync(IEnumerable<UserId> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a user by their email address
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all users (for admin purposes)
    /// </summary>
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
}