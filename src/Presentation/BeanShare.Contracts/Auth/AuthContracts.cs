namespace BeanShare.Contracts.Auth;

public sealed record LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public sealed record RegisterRequest
{
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required string Password { get; init; }
}

public sealed record AuthResponse
{
    public required string Token { get; init; }
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public string? PictureUrl { get; init; }
    public required string Provider { get; init; }
    public required DateTime ExpiresAt { get; init; }
}

public sealed record ExternalAuthRequest
{
    public required string Provider { get; init; }
    public required string IdToken { get; init; }
    public string? AccessToken { get; init; }
}
