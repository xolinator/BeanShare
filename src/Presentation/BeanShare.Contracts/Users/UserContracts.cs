namespace BeanShare.Contracts.Users;

public sealed record UpdatePreferredCurrencyRequest
{
    public string? CurrencyCode { get; init; }
}

public sealed record GetCurrentUserResponse
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public string? PictureUrl { get; init; }
    public string? PreferredCurrencyCode { get; init; }
}
