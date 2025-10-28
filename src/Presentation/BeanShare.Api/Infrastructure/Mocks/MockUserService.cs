using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;

namespace BeanShare.Api.Infrastructure.Mocks;

/// <summary>
/// Mock implementation of IUserService for testing
/// </summary>
public sealed class MockUserService : IUserService
{
    private readonly List<User> _users = new();

    public MockUserService()
    {
        SeedData();
    }

    private void SeedData()
    {
        // Seed with a test user that matches the MockUserContext
        var userId = new UserId(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var user = User.CreateWithPassword(
            "test@example.com",
            "Test User",
            "hashed_password");

        // Use reflection to set the ID since it's private
        var idProperty = typeof(User).GetProperty(nameof(User.Id));
        idProperty?.SetValue(user, userId);

        _users.Add(user);

        // Add additional test users
        var userId2 = new UserId(Guid.Parse("33333333-3333-3333-3333-333333333333"));
        var user2 = User.CreateWithPassword(
            "user2@example.com",
            "Second User",
            "hashed_password");

        idProperty?.SetValue(user2, userId2);
        _users.Add(user2);
    }

    public Task<User?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        return Task.FromResult(user);
    }

    public Task<IReadOnlyList<User>> GetByIdsAsync(IEnumerable<UserId> userIds, CancellationToken cancellationToken = default)
    {
        var idList = userIds.ToList();
        var result = _users.Where(u => idList.Contains(u.Id)).ToList();
        return Task.FromResult<IReadOnlyList<User>>(result);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Task.FromResult<User?>(null);
        }

        var emailLower = email.ToLowerInvariant();
        var user = _users.FirstOrDefault(u => u.Email.ToLowerInvariant() == emailLower);
        return Task.FromResult(user);
    }

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<User>>(_users.OrderBy(u => u.Name).ToList());
    }
}
