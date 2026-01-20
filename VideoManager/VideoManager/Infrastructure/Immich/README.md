# Immich Client

This directory contains the Immich API client implementation for VideoManager.

## Overview

The Immich client is automatically generated from the [Immich OpenAPI specification](https://raw.githubusercontent.com/immich-app/immich/master/open-api/immich-openapi-specs.json) using Microsoft Kiota. The generated client is wrapped in a clean, domain-specific interface that exposes only the functionality relevant to our video management needs.

## Structure

```
Infrastructure/Immich/
├── Generated/              # Auto-generated Kiota client (regenerated on build)
├── IImmichService.cs      # Service interface
├── ImmichService.cs       # Service implementation with mapping logic
├── Models.cs              # Domain models (ImmichAsset, ImmichExifInfo, etc.)
├── ImmichOptions.cs       # Configuration options
└── ServiceCollectionExtensions.cs  # DI registration
```

## Usage

### Configuration

Add Immich client to your services in `Program.cs`:

```csharp
builder.Services.AddImmichClient(options =>
{
    options.BaseUrl = "https://your-immich-instance.com/api";
    options.ApiKey = "your-api-key";
});
```

### Service Interface

The `IImmichService` provides the following methods:

- `GetAssetAsync(Guid assetId)` - Get a single asset by ID
- `GetAssetsAsync(AssetType? type, int? limit)` - Search for assets with optional filtering
- `GetVideoAssetsAsync(int? limit)` - Get video assets only
- `UpdateAssetMetadataAsync(Guid assetId, UpdateAssetMetadata metadata)` - Update a single asset's metadata
- `UpdateAssetsMetadataAsync(IEnumerable<Guid> assetIds, UpdateAssetMetadata metadata)` - Update multiple assets' metadata

### Example

```csharp
public class VideoService
{
    private readonly IImmichService _immichService;

    public VideoService(IImmichService immichService)
    {
        _immichService = immichService;
    }

    public async Task<IEnumerable<ImmichAsset>> GetAllVideosAsync()
    {
        return await _immichService.GetVideoAssetsAsync(limit: 100);
    }

    public async Task UpdateVideoDescriptionAsync(Guid videoId, string description)
    {
        await _immichService.UpdateAssetMetadataAsync(videoId, new UpdateAssetMetadata
        {
            Description = description
        });
    }
}
```

## Domain Models

### ImmichAsset

Represents an asset (photo/video) in Immich:

- `Id` - Unique identifier
- `OriginalFileName` - Original file name
- `OriginalPath` - Path on disk
- `Type` - AssetType enum (Image, Video, Audio, Other)
- `CreatedAt` - Upload timestamp
- `Duration` - Video duration (for videos)
- `Width/Height` - Dimensions
- `Description` - User-provided description
- `IsFavorite` - Favorite status
- `Rating` - User rating
- `ExifInfo` - Detailed EXIF metadata

### UpdateAssetMetadata

Used to update asset metadata:

- `Description` - Update description
- `IsFavorite` - Toggle favorite status
- `Rating` - Set rating (0-5)
- `DateTimeOriginal` - Override original date/time
- `Latitude/Longitude` - Update GPS coordinates

## Client Regeneration

The Kiota client is automatically regenerated on every build. The generation process:

1. Restores dotnet tools (including Kiota)
2. Downloads the latest OpenAPI spec from GitHub
3. Generates the client in `Infrastructure/Immich/Generated/`

To manually regenerate the client:

```bash
cd VideoManager
dotnet tool restore
dotnet tool run kiota generate -l CSharp -c ImmichClient -n VideoManager.Infrastructure.Immich.Generated -o VideoManager\Infrastructure\Immich\Generated -d https://raw.githubusercontent.com/immich-app/immich/master/open-api/immich-openapi-specs.json
```

## Notes

- The generated client code should not be modified manually
- Only the wrapped interface (`IImmichService`) should be used in application code
- The client automatically handles authentication using the API key header
