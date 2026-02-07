namespace VManBackend.Common.Models;

public class ItemPerson
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;
    
    public string ProviderName { get; set; } = null!;
    public string ProviderItemId { get; set; } = null!;
    public Item Item { get; set; } = null!;
    
    public DateTimeOffset CreatedAt { get; set; }
}
