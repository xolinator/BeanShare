using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;

namespace BeanShare.Application.Abstractions;

public interface ISemiAuthQrDeviceTokenRepository
{
    Task<SemiAuthQrDeviceToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task<SemiAuthQrDeviceToken?> GetByUserAndDeviceAsync(UserId userId, string deviceIdHash, CancellationToken ct = default);
    Task AddAsync(SemiAuthQrDeviceToken token, CancellationToken ct = default);
    Task UpdateAsync(SemiAuthQrDeviceToken token, CancellationToken ct = default);
}
