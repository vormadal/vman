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

            // Sync people after assets are synced
            _logger.LogInformation("Starting people sync from Immich");
            await SyncPeopleAsync(now, cancellationToken);

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

    private async Task SyncPeopleAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        // Get all people from Immich (including hidden)
        var peopleResponse = await _immichService.GetPeopleAsync(withHidden: true, cancellationToken);
        if (peopleResponse?.People == null || peopleResponse.People.Count == 0)
        {
            _logger.LogInformation("No people found in Immich");
            return;
        }

        _logger.LogInformation("Syncing {Count} people from Immich", peopleResponse.People.Count);

        var personIds = peopleResponse.People.Select(p => p.Id ?? string.Empty).Where(id => !string.IsNullOrEmpty(id)).ToList();

        // Get existing people
        var existingPeople = await _db.People
            .Where(p => p.ProviderName == "immich" && personIds.Contains(p.ProviderItemId))
            .ToDictionaryAsync(p => p.ProviderItemId, cancellationToken);

        foreach (var personDto in peopleResponse.People)
        {
            if (string.IsNullOrEmpty(personDto.Id) || string.IsNullOrEmpty(personDto.Name))
                continue;

            DateOnly? birthDate = null;
            if (personDto.BirthDate != null)
            {
                // Kiota's Date type has Year, Month, Day properties
                birthDate = new DateOnly(
                    personDto.BirthDate.Value.Year,
                    personDto.BirthDate.Value.Month,
                    personDto.BirthDate.Value.Day
                );
            }

            if (existingPeople.TryGetValue(personDto.Id, out var existingPerson))
            {
                // Update existing person
                existingPerson.Name = personDto.Name;
                existingPerson.BirthDate = birthDate;
                existingPerson.ThumbnailPath = personDto.ThumbnailPath;
                existingPerson.IsFavorite = personDto.IsFavorite ?? false;
                existingPerson.IsHidden = personDto.IsHidden ?? false;
                existingPerson.UpdatedAt = personDto.UpdatedAt ?? DateTimeOffset.UtcNow;
                existingPerson.LastSyncedAt = now;
            }
            else
            {
                // Create new person
                var newPerson = new Person
                {
                    Id = Guid.NewGuid(),
                    ProviderName = "immich",
                    ProviderItemId = personDto.Id,
                    Name = personDto.Name,
                    BirthDate = birthDate,
                    ThumbnailPath = personDto.ThumbnailPath,
                    IsFavorite = personDto.IsFavorite ?? false,
                    IsHidden = personDto.IsHidden ?? false,
                    UpdatedAt = personDto.UpdatedAt ?? DateTimeOffset.UtcNow,
                    LastSyncedAt = now
                };
                _db.People.Add(newPerson);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        // Now sync the person-to-asset relationships
        _logger.LogInformation("Syncing person-to-asset relationships");
        await SyncPersonAssetsAsync(now, cancellationToken);
    }

    private async Task SyncPersonAssetsAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        const int AssetBatchSize = 1000;
        
        // Get all people from database
        var allPeople = await _db.People
            .Where(p => p.ProviderName == "immich")
            .ToListAsync(cancellationToken);

        foreach (var person in allPeople.Where(p => Guid.TryParse(p.ProviderItemId, out _)))
        {
            if (!Guid.TryParse(person.ProviderItemId, out var personGuid))
                continue;

            // Get existing ItemPerson relationships for this person
            var existingRelationships = await _db.ItemPeople
                .Where(ip => ip.PersonId == person.Id)
                .Select(ip => new { ip.ItemId, ip.ProviderItemId })
                .ToListAsync(cancellationToken);
            
            var existingItemIds = existingRelationships.Select(r => r.ItemId).ToHashSet();
            var existingProviderIds = existingRelationships.Select(r => r.ProviderItemId).ToHashSet();

            // Process assets in batches to avoid large IN clauses
            var assetBatch = new List<string>();
            await foreach (var asset in _immichService.GetAssetsForPersonAsync(personGuid, cancellationToken))
            {
                assetBatch.Add(asset.Id.ToString());

                if (assetBatch.Count >= AssetBatchSize)
                {
                    await ProcessAssetBatchForPersonAsync(person, assetBatch, existingItemIds, existingProviderIds, now, cancellationToken);
                    assetBatch.Clear();
                }
            }

            // Process remaining assets
            if (assetBatch.Count > 0)
            {
                await ProcessAssetBatchForPersonAsync(person, assetBatch, existingItemIds, existingProviderIds, now, cancellationToken);
            }

            // Remove relationships that no longer exist in Immich (only need to check against provider IDs we've seen)
            var allProcessedProviderIds = existingProviderIds.ToHashSet();
            var relationshipsToRemove = existingRelationships
                .Where(r => allProcessedProviderIds.Contains(r.ProviderItemId))
                .Select(r => r.ItemId)
                .ToHashSet();
            
            if (relationshipsToRemove.Count > 0)
            {
                var toRemove = await _db.ItemPeople
                    .Where(ip => ip.PersonId == person.Id && !relationshipsToRemove.Contains(ip.ItemId))
                    .ToListAsync(cancellationToken);
                
                _db.ItemPeople.RemoveRange(toRemove);
            }

            // Save changes per person to keep transaction sizes bounded
            await _db.SaveChangesAsync(cancellationToken);
            
            // Clear change tracker to prevent memory bloat
            _db.ChangeTracker.Clear();
        }

        _logger.LogInformation("Person-to-asset relationships synced");
    }

    private async Task ProcessAssetBatchForPersonAsync(
        Person person,
        List<string> assetIds,
        HashSet<Guid> existingItemIds,
        HashSet<string> existingProviderIds,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        // Find items that exist in our database for this batch
        var existingItems = await _db.Items
            .Where(i => i.ProviderName == "immich" && assetIds.Contains(i.ProviderItemId))
            .Select(i => new { i.Id, i.ProviderItemId })
            .ToListAsync(cancellationToken);

        // Add new relationships
        foreach (var item in existingItems.Where(item => !existingItemIds.Contains(item.Id)))
        {
            var itemPerson = new ItemPerson
            {
                Id = Guid.NewGuid(),
                PersonId = person.Id,
                ItemId = item.Id,
                ProviderName = "immich",
                ProviderItemId = item.ProviderItemId,
                CreatedAt = now
            };
            _db.ItemPeople.Add(itemPerson);
            existingItemIds.Add(item.Id);
        }
        
        // Track provider IDs we've seen
        foreach (var providerItemId in assetIds)
        {
            existingProviderIds.Add(providerItemId);
        }
    }
}
