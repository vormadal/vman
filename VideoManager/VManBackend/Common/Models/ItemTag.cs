namespace VManBackend.Common.Models;

public class ItemTag
{
    public Guid Id { get; set; }
    public Guid TagId { get; set; }
    public required string ProviderName { get; set; }
    public required string ProviderItemId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public Tag Tag { get; set; } = null!;
}
