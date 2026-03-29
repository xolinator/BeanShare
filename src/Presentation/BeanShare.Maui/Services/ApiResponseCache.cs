using System.Collections.Concurrent;

namespace BeanShare.Maui.Services;

/// <summary>
/// Short-lived in-memory cache for API responses to reduce latency on repeated reads.
/// Entries expire after a configurable TTL (default 30 seconds).
/// Write operations (POST/PUT/DELETE) should call Invalidate() to clear stale data.
/// </summary>
public sealed class ApiResponseCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _entries = new();
    private readonly TimeSpan _defaultTtl = TimeSpan.FromSeconds(30);
    private const int MaxCacheSize = 500;
    private DateTime _lastCleanup = DateTime.UtcNow;
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromSeconds(60);

    public T? Get<T>(string key) where T : class
    {
        // Periodically clean up expired entries
        var now = DateTime.UtcNow;
        if (now - _lastCleanup > CleanupInterval)
        {
            _lastCleanup = now;
            foreach (var kvp in _entries)
            {
                if (kvp.Value.ExpiresAt <= now)
                {
                    _entries.TryRemove(kvp.Key, out _);
                }
            }
        }

        if (_entries.TryGetValue(key, out var entry) && entry.ExpiresAt > now)
        {
            return entry.Value as T;
        }
        return null;
    }

    /// <summary>
    /// Returns cached data even if expired (stale). Callers should refresh in background.
    /// Returns null only if key was never cached.
    /// </summary>
    public T? GetStale<T>(string key) where T : class
    {
        if (_entries.TryGetValue(key, out var entry))
            return entry.Value as T;
        return null;
    }

    public void Set<T>(string key, T value, TimeSpan? ttl = null) where T : class
    {
        if (_entries.Count > MaxCacheSize)
        {
            _entries.Clear();
        }

        _entries[key] = new CacheEntry(value, DateTime.UtcNow + (ttl ?? _defaultTtl));
    }

    public void Invalidate(string keyPrefix)
    {
        foreach (var key in _entries.Keys)
        {
            if (key.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                _entries.TryRemove(key, out _);
            }
        }
    }

    public void Clear()
    {
        _entries.Clear();
    }

    private sealed record CacheEntry(object Value, DateTime ExpiresAt);
}
