using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Repositories;

public sealed class SemiAuthQrDeviceTokenRepository(BeanShareDbContext context) : ISemiAuthQrDeviceTokenRepository
{
    private readonly BeanShareDbContext _context = context;

    public async Task<SemiAuthQrDeviceToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return await _context.SemiAuthQrDeviceTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);
    }

    public async Task<SemiAuthQrDeviceToken?> GetByUserAndDeviceAsync(UserId userId, string deviceIdHash, CancellationToken ct = default)
    {
        return await _context.SemiAuthQrDeviceTokens
            .FirstOrDefaultAsync(x => x.UserId == userId && x.DeviceIdHash == deviceIdHash, ct);
    }

    public async Task AddAsync(SemiAuthQrDeviceToken token, CancellationToken ct = default)
    {
        await _context.SemiAuthQrDeviceTokens.AddAsync(token, ct);
    }

    public Task UpdateAsync(SemiAuthQrDeviceToken token, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
