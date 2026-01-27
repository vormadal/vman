namespace VManBackend.Infrastructure.Providers;

/// <summary>
/// Filter options for querying media items
/// </summary>
public class MediaItemFilter
{
    public MediaType? Type { get; set; }
    public bool? IsFavorite { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? SearchQuery { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool Descending { get; set; } = true;
}

/// <summary>
/// Paginated result for media items
/// </summary>
public class MediaItemResult
{
    public List<MediaItem> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
