namespace VManBackend.Infrastructure.Providers;

/// <summary>
/// Interface for media providers (Immich, OneDrive, etc.)
/// </summary>
public interface IMediaProvider
{
    /// <summary>
    /// The type of this provider
    /// </summary>
    ProviderType ProviderType { get; }
    
    /// <summary>
    /// Get a paginated list of media items with optional filters
    /// </summary>
    Task<MediaItemResult> GetItemsAsync(MediaItemFilter filter, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a specific media item by its provider-specific ID
    /// </summary>
    Task<MediaItem?> GetItemByIdAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if the provider is available and properly configured
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}
