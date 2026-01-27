namespace VManBackend.Infrastructure.Providers;

/// <summary>
/// Unified media item model that works across all providers
/// </summary>
public class MediaItem
{
    public required string Id { get; set; }
    public required ProviderType Provider { get; set; }
    public required MediaType Type { get; set; }
    public required string OriginalFileName { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? PreviewUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? FileSizeBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? Duration { get; set; } // For videos (format: HH:MM:SS or ISO 8601)
    public string? Description { get; set; }
    public bool IsFavorite { get; set; }
    
    // Location data
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    
    // Camera/device info
    public string? Make { get; set; }
    public string? Model { get; set; }
    
    // Tags will be added separately from our database
    public List<string> Tags { get; set; } = new();
}

public enum MediaType
{
    Image,
    Video,
    Audio,
    Other
}
