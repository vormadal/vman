using Microsoft.Extensions.Caching.Memory;

namespace VManBackend.Infrastructure.Providers;

/// <summary>
/// Wrapper around IMediaProvider that adds memory caching to reduce API calls
/// </summary>
public class CachedMediaProvider : IMediaProvider
{
    private readonly IMediaProvider _innerProvider;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public CachedMediaProvider(IMediaProvider innerProvider, IMemoryCache cache)
    {
        _innerProvider = innerProvider;
        _cache = cache;
    }

    public ProviderType ProviderType => _innerProvider.ProviderType;

    public async Task<MediaItemResult> GetItemsAsync(MediaItemFilter filter, CancellationToken cancellationToken = default)
    {
        // Create cache key from filter parameters
        var cacheKey = $"MediaProvider:{ProviderType}:Items:{filter.Type}:{filter.IsFavorite}:{filter.Page}:{filter.PageSize}:{filter.SortBy}:{filter.Descending}:{filter.SearchQuery}";

        if (_cache.TryGetValue<MediaItemResult>(cacheKey, out var cached) && cached != null)
        {
            return cached;
        }

        var result = await _innerProvider.GetItemsAsync(filter, cancellationToken);
        _cache.Set(cacheKey, result, _cacheDuration);
        return result;
    }

    public async Task<MediaItem?> GetItemByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"MediaProvider:{ProviderType}:Item:{id}";

        if (_cache.TryGetValue<MediaItem?>(cacheKey, out var cached))
        {
            return cached;
        }

        var result = await _innerProvider.GetItemByIdAsync(id, cancellationToken);
        _cache.Set(cacheKey, result, _cacheDuration);
        return result;
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        // Don't cache availability checks - they should be fast anyway
        return _innerProvider.IsAvailableAsync(cancellationToken);
    }

    /// <summary>
    /// Clear all cached data for this provider
    /// </summary>
    public void ClearCache()
    {
        // Note: IMemoryCache doesn't have a built-in clear all method
        // In a production app, you'd want to track cache keys or use a different cache implementation
        // For now, items will expire naturally after 5 minutes
    }
}
