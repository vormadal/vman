namespace VManBackend.Common.Models;

public class SyncHistory
{
    public Guid Id { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public SyncStatus Status { get; set; }
    public int TotalAssets { get; set; }
    public int SyncedAssets { get; set; }
    public int FailedAssets { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AssetTypeFilter { get; set; }
    public Guid? UserId { get; set; }
    
    // Navigation property
    public User? User { get; set; }
}

public enum SyncStatus
{
    InProgress,
    Completed,
    Failed
}
