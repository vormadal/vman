namespace VManBackend.Common.Models;

public class Collection
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation property
    public ICollection<CollectionItem> CollectionItems { get; set; } = new List<CollectionItem>();
}
