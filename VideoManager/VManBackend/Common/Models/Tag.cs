namespace VManBackend.Common.Models;

public class Tag
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation property
    public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
}
