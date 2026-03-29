using System.Collections.Concurrent;

namespace BeanShare.SharedUi.Services;

/// <summary>
/// In-memory cache for resolved Active QR code data.
/// Falls back to cached results when the API/server is unreachable.
/// </summary>
public sealed class QrCodeResolveCache
{
    private readonly ConcurrentDictionary<Guid, QrConsumptionPayload> _cache = new();
    private const int MaxCacheSize = 200;

    public void Store(Guid qrCodeId, QrConsumptionPayload resolved)
    {
        if (_cache.Count > MaxCacheSize)
        {
            _cache.Clear();
        }

        _cache[qrCodeId] = resolved;
    }

    public QrConsumptionPayload? TryGet(Guid qrCodeId)
    {
        return _cache.TryGetValue(qrCodeId, out var cached) ? cached : null;
    }
}
