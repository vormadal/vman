using VManBackend.Infrastructure.Immich;
using VManBackend.Infrastructure.Immich.Generated.Models;

namespace VManBackend.Infrastructure.Providers;

/// <summary>
/// Immich media provider implementation
/// </summary>
public class ImmichMediaProvider : IMediaProvider
{
    private readonly IImmichService _immichService;

    public ImmichMediaProvider(IImmichService immichService)
    {
        _immichService = immichService;
    }

    public ProviderType ProviderType => ProviderType.Immich;

    public async Task<MediaItemResult> GetItemsAsync(MediaItemFilter filter, CancellationToken cancellationToken = default)
    {
        // Convert our MediaType to Immich AssetTypeEnum
        AssetType? immichType = filter.Type switch
        {
            MediaType.Image => AssetType.Image,
            MediaType.Video => AssetType.Video,
            MediaType.Audio => AssetType.Audio,
            MediaType.Other => AssetType.Other,
            _ => null
        };

        // Get assets from Immich
        var assets = await _immichService.GetAssetsAsync(immichType, null, cancellationToken);
        
        // Apply filters
        var filteredAssets = assets.AsEnumerable();
        
        if (filter.IsFavorite.HasValue)
        {
            filteredAssets = filteredAssets.Where(a => a.IsFavorite == filter.IsFavorite.Value);
        }
        
        if (filter.CreatedAfter.HasValue)
        {
            filteredAssets = filteredAssets.Where(a => a.FileCreatedAt >= filter.CreatedAfter.Value);
        }
        
        if (filter.CreatedBefore.HasValue)
        {
            filteredAssets = filteredAssets.Where(a => a.FileCreatedAt <= filter.CreatedBefore.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
        {
            var query = filter.SearchQuery.ToLowerInvariant();
            filteredAssets = filteredAssets.Where(a => 
                a.OriginalFileName.ToLowerInvariant().Contains(query) ||
                (a.Description != null && a.Description.ToLowerInvariant().Contains(query)));
        }

        // Sort
        filteredAssets = filter.SortBy?.ToLowerInvariant() switch
        {
            "filename" => filter.Descending 
                ? filteredAssets.OrderByDescending(a => a.OriginalFileName)
                : filteredAssets.OrderBy(a => a.OriginalFileName),
            _ => filter.Descending 
                ? filteredAssets.OrderByDescending(a => a.FileCreatedAt)
                : filteredAssets.OrderBy(a => a.FileCreatedAt)
        };

        var allItems = filteredAssets.ToList();
        var totalCount = allItems.Count;

        // Paginate
        var pagedItems = allItems
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(MapToMediaItem)
            .ToList();

        return new MediaItemResult
        {
            Items = pagedItems,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<MediaItem?> GetItemByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(id, out var assetId))
        {
            return null;
        }

        var asset = await _immichService.GetAssetAsync(assetId, cancellationToken);
        return asset == null ? null : MapToMediaItem(asset);
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to fetch a small number of assets to verify connection
            await _immichService.GetAssetsAsync(null, 1, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private MediaItem MapToMediaItem(ImmichAsset asset)
    {
        return new MediaItem
        {
            Id = asset.Id.ToString(),
            Provider = ProviderType.Immich,
            Type = MapAssetType(asset.Type),
            OriginalFileName = asset.OriginalFileName,
            ThumbnailUrl = $"/api/immich/assets/{asset.Id}/thumbnail",
            PreviewUrl = $"/api/immich/assets/{asset.Id}/preview",
            CreatedAt = (asset.FileCreatedAt ?? asset.CreatedAt).DateTime,
            UpdatedAt = null, // Immich doesn't track updates
            FileSizeBytes = null, // Not available in current model
            Width = asset.Width,
            Height = asset.Height,
            Duration = asset.Duration,
            Description = asset.Description,
            IsFavorite = asset.IsFavorite,
            Latitude = asset.ExifInfo?.Latitude,
            Longitude = asset.ExifInfo?.Longitude,
            City = asset.ExifInfo?.City,
            State = asset.ExifInfo?.State,
            Country = asset.ExifInfo?.Country,
            Make = asset.ExifInfo?.Make,
            Model = asset.ExifInfo?.Model
        };
    }

    private static MediaType MapAssetType(AssetType assetType)
    {
        return assetType switch
        {
            AssetType.Image => MediaType.Image,
            AssetType.Video => MediaType.Video,
            AssetType.Audio => MediaType.Audio,
            AssetType.Other => MediaType.Other,
            _ => MediaType.Other
        };
    }
}
