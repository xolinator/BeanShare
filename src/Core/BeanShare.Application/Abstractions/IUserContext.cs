using BeanShare.Domain.Common;

namespace BeanShare.Application.Abstractions;

public interface IUserContext
{
    UserId CurrentUserId { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
}