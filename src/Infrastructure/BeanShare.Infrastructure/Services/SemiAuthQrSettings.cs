using BeanShare.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace BeanShare.Infrastructure.Services;

public sealed class SemiAuthQrSettings(IOptions<SemiAuthQrOptions> options) : ISemiAuthQrSettings
{
    private readonly SemiAuthQrOptions _options = options.Value;

    public bool Enabled => _options.Enabled;

    public TimeSpan DeviceTokenLifetime => TimeSpan.FromHours(
        Math.Clamp(_options.DeviceTokenLifetimeHours, 1, 24 * 365));
}
