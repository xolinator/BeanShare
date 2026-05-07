namespace BeanShare.Application.Abstractions;

public interface ISemiAuthQrSettings
{
    bool Enabled { get; }
    TimeSpan DeviceTokenLifetime { get; }
}
