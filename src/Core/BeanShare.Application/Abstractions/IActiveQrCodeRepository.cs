using BeanShare.Domain.Entities;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Abstractions;

public interface IActiveQrCodeRepository
{
    Task<ActiveQrCode?> GetByIdAsync(ActiveQrCodeId id, CancellationToken cancellationToken = default);
    Task<ActiveQrCode?> GetSingleBySpecAsync(ISpec<ActiveQrCode> spec, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActiveQrCode>> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken cancellationToken = default);
    Task AddAsync(ActiveQrCode entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ActiveQrCode entity, CancellationToken cancellationToken = default);
}
