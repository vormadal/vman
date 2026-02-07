namespace VManBackend.Common.Models;

public class Person
{
    public Guid Id { get; set; }
    public required string ProviderName { get; set; }
    public required string ProviderItemId { get; set; }
    public required string Name { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? ThumbnailPath { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsHidden { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset LastSyncedAt { get; set; }

    // Navigation property
    public ICollection<ItemPerson> ItemPeople { get; set; } = new List<ItemPerson>();
}
