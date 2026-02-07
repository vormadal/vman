using VManBackend.Infrastructure.Providers;

namespace VManBackend.Common.Models;

public class Item
{
    public Guid Id { get; set; }
    public required string ProviderName { get; set; }
    public required string ProviderItemId { get; set; }
    public required string OriginalFileName { get; set; }
    public required MediaType Type { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastSyncedAt { get; set; }

    // Navigation properties
    public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
    public ICollection<ItemPerson> ItemPeople { get; set; } = new List<ItemPerson>();
}
