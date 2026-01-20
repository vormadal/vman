namespace VideoManager.Infrastructure.Immich;

public enum AssetType
{
    Image,
    Video,
    Audio,
    Other
}

public record ImmichAsset(
    Guid Id,
    string OriginalFileName,
    string OriginalPath,
    AssetType Type,
    DateTimeOffset CreatedAt,
    DateTimeOffset? FileCreatedAt,
    DateTimeOffset? LocalDateTime,
    string? Duration,
    int? Width,
    int? Height,
    string? Description,
    bool IsFavorite,
    bool IsArchived,
    double? Rating,
    ImmichExifInfo? ExifInfo
);

public record ImmichExifInfo(
    string? Make,
    string? Model,
    string? LensModel,
    double? FNumber,
    double? FocalLength,
    double? Iso,
    string? ExposureTime,
    double? Latitude,
    double? Longitude,
    string? City,
    string? State,
    string? Country
);

public record UpdateAssetMetadata(
    string? Description = null,
    bool? IsFavorite = null,
    double? Rating = null,
    DateTimeOffset? DateTimeOriginal = null,
    double? Latitude = null,
    double? Longitude = null
);
