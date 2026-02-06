using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Infrastructure.Immich;
using VManBackend.Infrastructure.Providers;

namespace VManBackend.Infrastructure.Sync;

public class ImmichSyncProcessor
{
    private readonly ApplicationDbContext _db;
    private readonly IImmichService _immichService;
    private readonly ILogger<ImmichSyncProcessor> _logger;
    private const int BatchSize = 100;
    private const int ProgressUpdateInterval = 50;

    public ImmichSyncProcessor(
        ApplicationDbContext db,
        IImmichService immichService,
        ILogger<ImmichSyncProcessor> logger)
    {
        _db = db;
        _immichService = immichService;
        _logger = logger;
    }

    public async Task ProcessAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var job = await _db.SyncJobs.FindAsync([jobId], cancellationToken);
        if (job == null)
        {
            _logger.LogWarning("Sync job {JobId} not found", jobId);
            return;
        }

        // Check if job was already cancelled
        if (job.Status == SyncJobStatus.Cancelled)
        {
            _logger.LogInformation("Sync job {JobId} was cancelled before processing", jobId);
            return;
        }

        try
        {
            job.Status = SyncJobStatus.InProgress;
            await _db.SaveChangesAsync(cancellationToken);

            // Get the total count first for progress reporting
            var totalCount = await _immichService.GetAssetsTotalCountAsync(cancellationToken: cancellationToken);
            job.TotalItems = totalCount;
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Syncing {Count} assets from Immich", totalCount);

            var processedCount = 0;
            var now = DateTimeOffset.UtcNow;
            var currentBatch = new List<Infrastructure.Immich.ImmichAsset>();

            // Stream assets and process in batches
            await foreach (var asset in _immichService.GetAssetsAsync(cancellationToken: cancellationToken).WithCancellation(cancellationToken))
            {
                currentBatch.Add(asset);

                if (currentBatch.Count >= BatchSize)
                {
                    // Check if job was cancelled during processing
                    await _db.Entry(job).ReloadAsync(cancellationToken);
                    if (job.Status == SyncJobStatus.Cancelled)
                    {
                        _logger.LogInformation("Sync job {JobId} was cancelled during processing", jobId);
                        return;
                    }

                    await ProcessBatchAsync(currentBatch, now, cancellationToken);
                    processedCount += currentBatch.Count;
                    currentBatch.Clear();

                    // Update progress
                    job.ProcessedItems = processedCount;
                    await _db.SaveChangesAsync(cancellationToken);
                    _logger.LogDebug("Sync progress: {Processed}/{Total}", processedCount, totalCount);
                }
            }

            // Process remaining items in the last batch
            if (currentBatch.Count > 0)
            {
                await ProcessBatchAsync(currentBatch, now, cancellationToken);
                processedCount += currentBatch.Count;
            }

            job.Status = SyncJobStatus.Completed;
            job.ProcessedItems = processedCount;
            job.CompletedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Sync completed: {Processed} items processed", processedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync job {JobId} failed", jobId);

            job.Status = SyncJobStatus.Failed;
            job.ErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
            job.CompletedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(CancellationToken.None);

            throw;
        }
    }

    private static MediaType MapAssetType(Immich.AssetType type)
    {
        return type switch
        {
            Immich.AssetType.Image => MediaType.Image,
            Immich.AssetType.Video => MediaType.Video,
            Immich.AssetType.Audio => MediaType.Audio,
            Immich.AssetType.Other => MediaType.Other,
            _ => MediaType.Other
        };
    }

    private async Task ProcessBatchAsync(List<Infrastructure.Immich.ImmichAsset> batch, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var providerItemIds = batch.Select(a => a.Id.ToString()).ToList();

        // Get existing items for this batch
        var existingItems = await _db.Items
            .Where(i => i.ProviderName == "immich" && providerItemIds.Contains(i.ProviderItemId))
            .ToDictionaryAsync(i => i.ProviderItemId, cancellationToken);

        foreach (var asset in batch)
        {
            var providerItemId = asset.Id.ToString();

            if (existingItems.TryGetValue(providerItemId, out var existingItem))
            {
                // Update existing item
                existingItem.OriginalFileName = asset.OriginalFileName;
                existingItem.Type = MapAssetType(asset.Type);
                existingItem.CreatedAt = asset.FileCreatedAt ?? asset.CreatedAt;
                existingItem.LastSyncedAt = now;
            }
            else
            {
                // Create new item
                var newItem = new Item
                {
                    Id = Guid.NewGuid(),
                    ProviderName = "immich",
                    ProviderItemId = providerItemId,
                    OriginalFileName = asset.OriginalFileName,
                    Type = MapAssetType(asset.Type),
                    CreatedAt = asset.FileCreatedAt ?? asset.CreatedAt,
                    LastSyncedAt = now
                };
                _db.Items.Add(newItem);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
