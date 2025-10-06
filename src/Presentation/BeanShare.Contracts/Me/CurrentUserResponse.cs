namespace BeanShare.Contracts.Me;

public sealed class CurrentUserResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = [];
}