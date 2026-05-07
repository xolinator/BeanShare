namespace BeanShare.Infrastructure.Services;

public sealed class SemiAuthQrOptions
{
    public const string SectionName = "Features:SemiAuthQr";

    public bool Enabled { get; set; }
    public int DeviceTokenLifetimeHours { get; set; } = 720; // 30 days
}
