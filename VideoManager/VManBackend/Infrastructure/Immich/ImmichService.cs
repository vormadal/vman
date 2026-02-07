using System.Runtime.CompilerServices;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using VManBackend.Infrastructure.Immich.Generated;
using VManBackend.Infrastructure.Immich.Generated.Models;

namespace VManBackend.Infrastructure.Immich;

public class ImmichService : IImmichService
{
    private readonly ImmichClient _client;

    public ImmichService(string baseUrl, string apiKey)
    {
        // var authProvider = new ApiKeyAuthenticationProvider("x-api-key", apiKey, ApiKeyAuthenticationProvider.KeyLocation.QueryParameter);
        var authProvider = new ApiKeyAuthenticationProvider("apiKey", apiKey, ApiKeyAuthenticationProvider.KeyLocation.QueryParameter);
        var adapter = new HttpClientRequestAdapter(authProvider)
        {
            BaseUrl = baseUrl
        };
        _client = new ImmichClient(adapter);
    }

    public ImmichService(ImmichClient client)
    {
        _client = client;
    }

    public async Task<ImmichAsset?> GetAssetAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        var asset = await _client.Assets[assetId].GetAsync(cancellationToken: cancellationToken);
        return asset == null ? null : MapToImmichAsset(asset);
    }

    public async Task<int> GetAssetsTotalCountAsync(AssetType? type = null, CancellationToken cancellationToken = default)
    {
        // Use the dedicated statistics endpoint for accurate counts
        var stats = await _client.Assets.Statistics.GetAsync(cancellationToken: cancellationToken);
        
        if (stats == null)
            return 0;
            
        // If a specific type is requested, return that count
        if (type.HasValue)
        {
            return type.Value switch
            {
                AssetType.Image => stats.Images ?? 0,
                AssetType.Video => stats.Videos ?? 0,
                _ => stats.Total ?? 0
            };
        }
        
        // Return total count for all assets
        return stats.Total ?? 0;
    }

    public async IAsyncEnumerable<ImmichAsset> GetAssetsAsync(AssetType? type = null, int? limit = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var remaining = limit;
        double? pageNumber = 1.0;

        while (pageNumber.HasValue)
        {
            var searchDto = new MetadataSearchDto
            {
                Type = type.HasValue ? MapToAssetTypeEnum(type.Value) : null,
                Size = remaining,
                Page = pageNumber
            };

            var searchResponse = await _client.Search.Metadata.PostAsync(searchDto, cancellationToken: cancellationToken);
            
            if (searchResponse?.Assets?.Items == null || searchResponse.Assets.Items.Count == 0)
                yield break;

            foreach (var asset in searchResponse.Assets.Items.Where(a => a != null))
            {
                yield return MapToImmichAsset(asset);
                
                // Update remaining count if limit is specified
                if (remaining.HasValue)
                {
                    remaining--;
                    if (remaining <= 0)
                        yield break;
                }
            }

            // Check if there's a next page
            if (string.IsNullOrEmpty(searchResponse.Assets.NextPage))
            {
                // No more pages
                pageNumber = null;
            }
            else if (double.TryParse(searchResponse.Assets.NextPage, out var parsedPage))
            {
                // NextPage is a numeric value, use it directly
                pageNumber = parsedPage;
            }
            else
            {
                // NextPage is not numeric - increment current page
                // This is a fallback for potential API changes
                pageNumber++;
            }
        }
    }

    public async Task<IEnumerable<ImmichAsset>> GetVideoAssetsAsync(int? limit = null, CancellationToken cancellationToken = default)
    {
        var results = new List<ImmichAsset>();
        await foreach (var asset in GetAssetsAsync(AssetType.Video, limit, cancellationToken))
        {
            results.Add(asset);
        }
        return results;
    }

    public async Task UpdateAssetMetadataAsync(Guid assetId, UpdateAssetMetadata metadata, CancellationToken cancellationToken = default)
    {
        var updateDto = new UpdateAssetDto
        {
            Description = metadata.Description,
            IsFavorite = metadata.IsFavorite,
            Rating = metadata.Rating,
            DateTimeOriginal = metadata.DateTimeOriginal?.ToString("O"),
            Latitude = metadata.Latitude,
            Longitude = metadata.Longitude
        };

        await _client.Assets[assetId].PutAsync(updateDto, cancellationToken: cancellationToken);
    }

    public async Task UpdateAssetsMetadataAsync(IEnumerable<Guid> assetIds, UpdateAssetMetadata metadata, CancellationToken cancellationToken = default)
    {
        foreach (var assetId in assetIds)
        {
            await UpdateAssetMetadataAsync(assetId, metadata, cancellationToken);
        }
    }

    private static ImmichAsset MapToImmichAsset(AssetResponseDto dto)
    {
        return new ImmichAsset(
            Id: Guid.Parse(dto.Id ?? throw new InvalidOperationException("Asset ID is required")),
            OriginalFileName: dto.OriginalFileName ?? string.Empty,
            OriginalPath: dto.OriginalPath ?? string.Empty,
            Type: MapFromAssetTypeEnum(dto.Type ?? AssetTypeEnum.OTHER),
            CreatedAt: dto.CreatedAt ?? DateTimeOffset.MinValue,
            FileCreatedAt: dto.FileCreatedAt,
            LocalDateTime: dto.LocalDateTime,
            Duration: dto.Duration,
            Width: (int?)dto.Width,
            Height: (int?)dto.Height,
            Description: dto.ExifInfo?.Description,
            IsFavorite: dto.IsFavorite ?? false,
            IsArchived: dto.IsArchived ?? false,
            Rating: dto.ExifInfo?.Rating,
            ExifInfo: dto.ExifInfo == null ? null : MapToImmichExifInfo(dto.ExifInfo)
        );
    }

    private static ImmichExifInfo MapToImmichExifInfo(ExifResponseDto dto)
    {
        return new ImmichExifInfo(
            Make: dto.Make,
            Model: dto.Model,
            LensModel: dto.LensModel,
            FNumber: dto.FNumber,
            FocalLength: dto.FocalLength,
            Iso: dto.Iso,
            ExposureTime: dto.ExposureTime,
            Latitude: dto.Latitude,
            Longitude: dto.Longitude,
            City: dto.City,
            State: dto.State,
            Country: dto.Country
        );
    }

    private static AssetType MapFromAssetTypeEnum(AssetTypeEnum type)
    {
        return type switch
        {
            AssetTypeEnum.IMAGE => AssetType.Image,
            AssetTypeEnum.VIDEO => AssetType.Video,
            AssetTypeEnum.AUDIO => AssetType.Audio,
            AssetTypeEnum.OTHER => AssetType.Other,
            _ => AssetType.Other
        };
    }

    private static AssetTypeEnum MapToAssetTypeEnum(AssetType type)
    {
        return type switch
        {
            AssetType.Image => AssetTypeEnum.IMAGE,
            AssetType.Video => AssetTypeEnum.VIDEO,
            AssetType.Audio => AssetTypeEnum.AUDIO,
            AssetType.Other => AssetTypeEnum.OTHER,
            _ => AssetTypeEnum.OTHER
        };
    }

    public async Task<Stream?> GetThumbnailAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        return await _client.Assets[assetId].Thumbnail.GetAsync(cancellationToken: cancellationToken);
    }

    public async Task<Stream?> GetPreviewAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        return await _client.Assets[assetId].Thumbnail.GetAsync(
            config => config.QueryParameters.SizeAsAssetMediaSize = AssetMediaSize.Preview,
            cancellationToken: cancellationToken);
    }

    public async Task<Stream?> GetOriginalAssetAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        return await _client.Assets[assetId].Original.GetAsync(cancellationToken: cancellationToken);
    }

    public async Task<PeopleResponseDto?> GetPeopleAsync(bool withHidden = false, CancellationToken cancellationToken = default)
    {
        return await _client.People.GetAsync(config =>
        {
            config.QueryParameters.WithHidden = withHidden;
        }, cancellationToken: cancellationToken);
    }

    public async Task<PersonResponseDto?> GetPersonAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        return await _client.People[personId].GetAsync(cancellationToken: cancellationToken);
    }

    public async IAsyncEnumerable<ImmichAsset> GetAssetsForPersonAsync(Guid personId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        double? pageNumber = 1.0;

        while (pageNumber.HasValue)
        {
            var searchDto = new MetadataSearchDto
            {
                PersonIds = new List<Guid?> { personId },
                Page = pageNumber
            };

            var searchResponse = await _client.Search.Metadata.PostAsync(searchDto, cancellationToken: cancellationToken);
            
            if (searchResponse?.Assets?.Items == null || searchResponse.Assets.Items.Count == 0)
                yield break;

            foreach (var asset in searchResponse.Assets.Items.Where(a => a != null))
            {
                yield return MapToImmichAsset(asset);
            }

            // Check if there's a next page
            if (string.IsNullOrEmpty(searchResponse.Assets.NextPage))
            {
                pageNumber = null;
            }
            else if (double.TryParse(searchResponse.Assets.NextPage, out var parsedPage))
            {
                pageNumber = parsedPage;
            }
            else
            {
                pageNumber++;
            }
        }
    }
}
