namespace VManBackend.Common.Models;

public class ImmichAsset
{
    public Guid Id { get; set; } // Immich asset ID
    public required string OriginalFileName { get; set; }
    public required string OriginalPath { get; set; }
    public AssetType AssetType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? FileCreatedAt { get; set; }
    public DateTimeOffset? LocalDateTime { get; set; }
    public string? Duration { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? Description { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsArchived { get; set; }
    public double? Rating { get; set; }
    public DateTimeOffset LastSyncedAt { get; set; }
    
    // Navigation property
    public ImmichExifData? ExifData { get; set; }
}

public enum AssetType
{
    Image,
    Video,
    Audio,
    Other
}
