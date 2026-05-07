namespace BeanShare.Contracts.SemiAuthQr;

public sealed class RevokeDeviceLogTokenRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
