using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;

namespace BeanShare.Api.Infrastructure.Mocks;

public sealed class MockUserContext : IUserContext
{
    public UserId CurrentUserId { get; } = new(Guid.Parse("11111111-1111-1111-1111-111111111111"));
    public string? Email { get; } = "test@coffeespace.local";
    public IReadOnlyCollection<string> Roles { get; } = new[] { "User" };
}