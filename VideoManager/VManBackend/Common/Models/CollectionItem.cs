namespace VManBackend.Common.Models;

public class CollectionItem
{
    public Guid Id { get; set; }
    public Guid CollectionId { get; set; }
    public required string ProviderName { get; set; }
    public required string ProviderItemId { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Collection Collection { get; set; } = null!;
}
