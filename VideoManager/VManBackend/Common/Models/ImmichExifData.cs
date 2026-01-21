namespace VManBackend.Common.Models;

public class ImmichExifData
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? LensModel { get; set; }
    public double? FNumber { get; set; }
    public double? FocalLength { get; set; }
    public double? Iso { get; set; }
    public string? ExposureTime { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    
    // Navigation property
    public ImmichAsset Asset { get; set; } = null!;
}
