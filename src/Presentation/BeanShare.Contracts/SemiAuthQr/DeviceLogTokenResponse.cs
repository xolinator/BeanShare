namespace BeanShare.Contracts.SemiAuthQr;

public sealed class DeviceLogTokenResponse
{
    public Guid TokenId { get; set; }
    public Guid UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
