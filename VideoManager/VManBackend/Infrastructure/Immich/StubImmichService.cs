using System.Runtime.CompilerServices;
using VManBackend.Infrastructure.Immich.Generated.Models;

namespace VManBackend.Infrastructure.Immich;

/// <summary>
/// Stub implementation of IImmichService that returns hardcoded test data.
/// Used for E2E testing without requiring a real Immich instance.
/// </summary>
public class StubImmichService : IImmichService
{
    private static readonly List<ImmichAsset> SampleAssets = new()
    {
        // Videos
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            OriginalFileName: "beach-sunset.mp4",
            OriginalPath: "/test/videos/beach-sunset.mp4",
            Type: AssetType.Video,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-30),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-30),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-30),
            Duration: "00:02:30",
            Width: 1920,
            Height: 1080,
            Description: "Beautiful sunset at the beach",
            IsFavorite: true,
            IsArchived: false,
            Rating: 5.0,
            ExifInfo: new ImmichExifInfo(
                Make: "DJI",
                Model: "DJI Mavic 3",
                LensModel: null,
                FNumber: 2.8,
                FocalLength: 24.0,
                Iso: 100,
                ExposureTime: "1/60",
                Latitude: 34.0522,
                Longitude: -118.2437,
                City: "Los Angeles",
                State: "California",
                Country: "USA"
            )
        ),
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000002"),
            OriginalFileName: "family-gathering.mp4",
            OriginalPath: "/test/videos/family-gathering.mp4",
            Type: AssetType.Video,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-20),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-20),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-20),
            Duration: "00:05:15",
            Width: 3840,
            Height: 2160,
            Description: "Family gathering at home",
            IsFavorite: false,
            IsArchived: false,
            Rating: 4.0,
            ExifInfo: null
        ),
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000003"),
            OriginalFileName: "mountain-hike.mp4",
            OriginalPath: "/test/videos/mountain-hike.mp4",
            Type: AssetType.Video,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-15),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-15),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-15),
            Duration: "00:03:45",
            Width: 1920,
            Height: 1080,
            Description: null,
            IsFavorite: true,
            IsArchived: false,
            Rating: null,
            ExifInfo: new ImmichExifInfo(
                Make: "GoPro",
                Model: "HERO11 Black",
                LensModel: null,
                FNumber: null,
                FocalLength: null,
                Iso: 200,
                ExposureTime: null,
                Latitude: 47.6062,
                Longitude: -122.3321,
                City: "Seattle",
                State: "Washington",
                Country: "USA"
            )
        ),
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000004"),
            OriginalFileName: "concert-performance.mp4",
            OriginalPath: "/test/videos/concert-performance.mp4",
            Type: AssetType.Video,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-10),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-10),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-10),
            Duration: "00:08:20",
            Width: 1920,
            Height: 1080,
            Description: "Live concert performance",
            IsFavorite: false,
            IsArchived: false,
            Rating: 3.5,
            ExifInfo: null
        ),
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000005"),
            OriginalFileName: "cooking-tutorial.mp4",
            OriginalPath: "/test/videos/cooking-tutorial.mp4",
            Type: AssetType.Video,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-5),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-5),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-5),
            Duration: "00:12:00",
            Width: 1280,
            Height: 720,
            Description: "How to make pasta carbonara",
            IsFavorite: true,
            IsArchived: false,
            Rating: 4.5,
            ExifInfo: null
        ),

        // Images
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000101"),
            OriginalFileName: "portrait-1.jpg",
            OriginalPath: "/test/images/portrait-1.jpg",
            Type: AssetType.Image,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-25),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-25),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-25),
            Duration: null,
            Width: 4000,
            Height: 6000,
            Description: "Portrait of John",
            IsFavorite: true,
            IsArchived: false,
            Rating: 5.0,
            ExifInfo: new ImmichExifInfo(
                Make: "Canon",
                Model: "EOS R5",
                LensModel: "RF 50mm F1.2 L USM",
                FNumber: 1.2,
                FocalLength: 50.0,
                Iso: 400,
                ExposureTime: "1/200",
                Latitude: 40.7128,
                Longitude: -74.0060,
                City: "New York",
                State: "New York",
                Country: "USA"
            )
        ),
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000102"),
            OriginalFileName: "portrait-2.jpg",
            OriginalPath: "/test/images/portrait-2.jpg",
            Type: AssetType.Image,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-24),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-24),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-24),
            Duration: null,
            Width: 4000,
            Height: 6000,
            Description: "Portrait of Jane",
            IsFavorite: true,
            IsArchived: false,
            Rating: 5.0,
            ExifInfo: new ImmichExifInfo(
                Make: "Canon",
                Model: "EOS R5",
                LensModel: "RF 50mm F1.2 L USM",
                FNumber: 1.2,
                FocalLength: 50.0,
                Iso: 400,
                ExposureTime: "1/200",
                Latitude: 40.7128,
                Longitude: -74.0060,
                City: "New York",
                State: "New York",
                Country: "USA"
            )
        ),
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000103"),
            OriginalFileName: "landscape-1.jpg",
            OriginalPath: "/test/images/landscape-1.jpg",
            Type: AssetType.Image,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-22),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-22),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-22),
            Duration: null,
            Width: 6000,
            Height: 4000,
            Description: "Mountain landscape",
            IsFavorite: false,
            IsArchived: false,
            Rating: 4.0,
            ExifInfo: new ImmichExifInfo(
                Make: "Nikon",
                Model: "Z9",
                LensModel: "NIKKOR Z 24-70mm f/2.8 S",
                FNumber: 8.0,
                FocalLength: 24.0,
                Iso: 100,
                ExposureTime: "1/250",
                Latitude: 46.8523,
                Longitude: 8.0512,
                City: "Interlaken",
                State: "Bern",
                Country: "Switzerland"
            )
        ),
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000104"),
            OriginalFileName: "cityscape-1.jpg",
            OriginalPath: "/test/images/cityscape-1.jpg",
            Type: AssetType.Image,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-18),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-18),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-18),
            Duration: null,
            Width: 5000,
            Height: 3333,
            Description: "City lights at night",
            IsFavorite: true,
            IsArchived: false,
            Rating: 4.5,
            ExifInfo: new ImmichExifInfo(
                Make: "Sony",
                Model: "A7R V",
                LensModel: "FE 24-105mm F4 G OSS",
                FNumber: 4.0,
                FocalLength: 35.0,
                Iso: 1600,
                ExposureTime: "1/30",
                Latitude: 35.6762,
                Longitude: 139.6503,
                City: "Tokyo",
                State: "Tokyo",
                Country: "Japan"
            )
        ),
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000105"),
            OriginalFileName: "wildlife-1.jpg",
            OriginalPath: "/test/images/wildlife-1.jpg",
            Type: AssetType.Image,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-14),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-14),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-14),
            Duration: null,
            Width: 4500,
            Height: 3000,
            Description: "Eagle in flight",
            IsFavorite: false,
            IsArchived: false,
            Rating: null,
            ExifInfo: new ImmichExifInfo(
                Make: "Canon",
                Model: "EOS R3",
                LensModel: "RF 100-500mm F4.5-7.1 L IS USM",
                FNumber: 5.6,
                FocalLength: 500.0,
                Iso: 3200,
                ExposureTime: "1/2000",
                Latitude: null,
                Longitude: null,
                City: null,
                State: null,
                Country: null
            )
        ),
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000106"),
            OriginalFileName: "architecture-1.jpg",
            OriginalPath: "/test/images/architecture-1.jpg",
            Type: AssetType.Image,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-12),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-12),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-12),
            Duration: null,
            Width: 6000,
            Height: 4000,
            Description: "Modern building design",
            IsFavorite: false,
            IsArchived: false,
            Rating: 3.0,
            ExifInfo: null
        ),
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000107"),
            OriginalFileName: "food-1.jpg",
            OriginalPath: "/test/images/food-1.jpg",
            Type: AssetType.Image,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-8),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-8),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-8),
            Duration: null,
            Width: 4000,
            Height: 3000,
            Description: "Gourmet pasta dish",
            IsFavorite: true,
            IsArchived: false,
            Rating: 4.0,
            ExifInfo: null
        ),
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000108"),
            OriginalFileName: "abstract-1.jpg",
            OriginalPath: "/test/images/abstract-1.jpg",
            Type: AssetType.Image,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-6),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-6),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-6),
            Duration: null,
            Width: 3000,
            Height: 3000,
            Description: null,
            IsFavorite: false,
            IsArchived: true,
            Rating: null,
            ExifInfo: null
        ),
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000109"),
            OriginalFileName: "street-1.jpg",
            OriginalPath: "/test/images/street-1.jpg",
            Type: AssetType.Image,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-3),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-3),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-3),
            Duration: null,
            Width: 5000,
            Height: 3333,
            Description: "Street photography",
            IsFavorite: false,
            IsArchived: false,
            Rating: 3.5,
            ExifInfo: new ImmichExifInfo(
                Make: "Fujifilm",
                Model: "X-T5",
                LensModel: "XF 23mm F1.4 R",
                FNumber: 2.0,
                FocalLength: 23.0,
                Iso: 800,
                ExposureTime: "1/125",
                Latitude: 51.5074,
                Longitude: -0.1278,
                City: "London",
                State: "England",
                Country: "UK"
            )
        ),
        new ImmichAsset(
            Id: Guid.Parse("00000000-0000-0000-0000-000000000110"),
            OriginalFileName: "nature-1.jpg",
            OriginalPath: "/test/images/nature-1.jpg",
            Type: AssetType.Image,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-1),
            FileCreatedAt: DateTimeOffset.UtcNow.AddDays(-1),
            LocalDateTime: DateTimeOffset.UtcNow.AddDays(-1),
            Duration: null,
            Width: 6000,
            Height: 4000,
            Description: "Forest pathway",
            IsFavorite: true,
            IsArchived: false,
            Rating: 5.0,
            ExifInfo: new ImmichExifInfo(
                Make: "Nikon",
                Model: "Z9",
                LensModel: "NIKKOR Z 24-120mm f/4 S",
                FNumber: 8.0,
                FocalLength: 35.0,
                Iso: 200,
                ExposureTime: "1/125",
                Latitude: 48.8566,
                Longitude: 2.3522,
                City: "Paris",
                State: "Île-de-France",
                Country: "France"
            )
        )
    };

    private static readonly List<PersonResponseDto> SamplePeople = new()
    {
        new PersonResponseDto
        {
            Id = "00000000-0000-0000-0000-000000000201",
            Name = "John Doe",
            BirthDate = new Microsoft.Kiota.Abstractions.Date(1990, 5, 15),
            ThumbnailPath = "/test/people/john-doe.jpg",
            IsFavorite = true,
            IsHidden = false,
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-20)
        },
        new PersonResponseDto
        {
            Id = "00000000-0000-0000-0000-000000000202",
            Name = "Jane Smith",
            BirthDate = new Microsoft.Kiota.Abstractions.Date(1992, 8, 22),
            ThumbnailPath = "/test/people/jane-smith.jpg",
            IsFavorite = true,
            IsHidden = false,
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-19)
        }
    };

    // Map people to their assets
    private static readonly Dictionary<string, List<Guid>> PeopleAssets = new()
    {
        ["00000000-0000-0000-0000-000000000201"] = new List<Guid>
        {
            Guid.Parse("00000000-0000-0000-0000-000000000101"), // portrait-1.jpg
            Guid.Parse("00000000-0000-0000-0000-000000000002")  // family-gathering.mp4
        },
        ["00000000-0000-0000-0000-000000000202"] = new List<Guid>
        {
            Guid.Parse("00000000-0000-0000-0000-000000000102"), // portrait-2.jpg
            Guid.Parse("00000000-0000-0000-0000-000000000002")  // family-gathering.mp4
        }
    };

    public Task<ImmichAsset?> GetAssetAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        var asset = SampleAssets.FirstOrDefault(a => a.Id == assetId);
        return Task.FromResult(asset);
    }

    public async IAsyncEnumerable<ImmichAsset> GetAssetsAsync(
        AssetType? type = null,
        int? limit = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var assets = SampleAssets.AsEnumerable();

        if (type.HasValue)
        {
            assets = assets.Where(a => a.Type == type.Value);
        }

        if (limit.HasValue)
        {
            assets = assets.Take(limit.Value);
        }

        foreach (var asset in assets)
        {
            await Task.Yield(); // Make it truly async
            yield return asset;
        }
    }

    public Task<int> GetAssetsTotalCountAsync(AssetType? type = null, CancellationToken cancellationToken = default)
    {
        if (type.HasValue)
        {
            return Task.FromResult(SampleAssets.Count(a => a.Type == type.Value));
        }

        return Task.FromResult(SampleAssets.Count);
    }

    public Task<IEnumerable<ImmichAsset>> GetVideoAssetsAsync(int? limit = null, CancellationToken cancellationToken = default)
    {
        var videos = SampleAssets.Where(a => a.Type == AssetType.Video);

        if (limit.HasValue)
        {
            videos = videos.Take(limit.Value);
        }

        return Task.FromResult(videos.AsEnumerable());
    }

    public Task UpdateAssetMetadataAsync(Guid assetId, UpdateAssetMetadata metadata, CancellationToken cancellationToken = default)
    {
        // No-op in stub - just return success
        return Task.CompletedTask;
    }

    public Task UpdateAssetsMetadataAsync(IEnumerable<Guid> assetIds, UpdateAssetMetadata metadata, CancellationToken cancellationToken = default)
    {
        // No-op in stub - just return success
        return Task.CompletedTask;
    }

    public Task<Stream?> GetThumbnailAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        // Return a minimal 1x1 PNG (smallest valid PNG)
        var pngBytes = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, // IHDR chunk
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, // 1x1 pixels
            0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, // RGBA, etc.
            0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, // IDAT chunk
            0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00, // compressed data
            0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00, // ...
            0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, // IEND chunk
            0x42, 0x60, 0x82
        };

        Stream stream = new MemoryStream(pngBytes);
        return Task.FromResult<Stream?>(stream);
    }

    public Task<Stream?> GetPreviewAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        // Return same minimal PNG as thumbnail
        return GetThumbnailAsync(assetId, cancellationToken);
    }

    public Task<Stream?> GetOriginalAssetAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        // Return same minimal PNG as thumbnail (for E2E tests, actual file content doesn't matter)
        return GetThumbnailAsync(assetId, cancellationToken);
    }

    public Task<PeopleResponseDto?> GetPeopleAsync(bool withHidden = false, CancellationToken cancellationToken = default)
    {
        var people = SamplePeople.AsEnumerable();

        if (!withHidden)
        {
            people = people.Where(p => p.IsHidden == false);
        }

        var response = new PeopleResponseDto
        {
            People = people.ToList(),
            Total = people.Count(),
            Hidden = SamplePeople.Count(p => p.IsHidden == true),
            HasNextPage = false
        };

        return Task.FromResult<PeopleResponseDto?>(response);
    }

    public Task<PersonResponseDto?> GetPersonAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        var person = SamplePeople.FirstOrDefault(p => p.Id == personId.ToString());
        return Task.FromResult(person);
    }

    public async IAsyncEnumerable<ImmichAsset> GetAssetsForPersonAsync(
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        if (PeopleAssets.TryGetValue(personId.ToString(), out var assetIds))
        {
            foreach (var assetId in assetIds)
            {
                var asset = SampleAssets.FirstOrDefault(a => a.Id == assetId);
                if (asset != null)
                {
                    await Task.Yield(); // Make it truly async
                    yield return asset;
                }
            }
        }
    }
}
