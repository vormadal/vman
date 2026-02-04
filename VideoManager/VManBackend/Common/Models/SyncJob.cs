namespace VManBackend.Common.Models;

public class SyncJob
{
    public Guid Id { get; set; }
    public required string ProviderName { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public SyncJobStatus Status { get; set; }
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum SyncJobStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled
}
