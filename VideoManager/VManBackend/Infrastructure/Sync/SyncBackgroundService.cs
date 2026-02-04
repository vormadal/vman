namespace VManBackend.Infrastructure.Sync;

public class SyncBackgroundService : BackgroundService
{
    private readonly SyncChannel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncBackgroundService> _logger;

    public SyncBackgroundService(
        SyncChannel channel,
        IServiceProvider serviceProvider,
        ILogger<SyncBackgroundService> logger)
    {
        _channel = channel;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sync background service started");

        await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Processing sync job {JobId} for provider {Provider}",
                    request.JobId, request.ProviderName);

                await using var scope = _serviceProvider.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<ImmichSyncProcessor>();
                await processor.ProcessAsync(request.JobId, stoppingToken);

                _logger.LogInformation("Completed sync job {JobId}", request.JobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sync job {JobId}", request.JobId);

                // Ensure job status is updated even if the processor failed early
                await TryUpdateJobStatusOnError(request.JobId, ex.Message);
            }
        }

        _logger.LogInformation("Sync background service stopped");
    }

    private async Task TryUpdateJobStatusOnError(Guid jobId, string errorMessage)
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<Common.Data.ApplicationDbContext>();

            var job = await db.SyncJobs.FindAsync([jobId]);
            if (job != null && job.Status != Common.Models.SyncJobStatus.Failed)
            {
                job.Status = Common.Models.SyncJobStatus.Failed;
                job.ErrorMessage = errorMessage.Length > 2000 ? errorMessage[..2000] : errorMessage;
                job.CompletedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update job status for {JobId}", jobId);
        }
    }
}
