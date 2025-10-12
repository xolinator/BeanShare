using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;

namespace BeanShare.BlazorWeb.Services;

/// <summary>
/// Development implementation of IUserContext service
/// Uses hardcoded user ID for rapid prototyping during development phase
/// </summary>
public class MockUserContext : IUserContext
{
    public UserId CurrentUserId => new UserId(Guid.Parse("11111111-1111-1111-1111-111111111111"));

    public string Email => "test@beanshare.com";

    public IReadOnlyCollection<string> Roles => new[] { "User" };
}
