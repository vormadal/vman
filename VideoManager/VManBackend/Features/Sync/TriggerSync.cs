using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Infrastructure.Sync;
using VManBackend.Mediator;

namespace VManBackend.Features.Sync;

public static class TriggerSync
{
    public record Request(string Provider = "immich") : IRequest<Response?>;

    public record Response(Guid JobId, string Status, string Message);

    public static class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (string.IsNullOrWhiteSpace(request.Provider))
            {
                error = "Provider is required";
                return false;
            }

            if (request.Provider != "immich")
            {
                error = "Only 'immich' provider is currently supported";
                return false;
            }

            error = null;
            return true;
        }
    }

    public class Handler(ApplicationDbContext db, SyncChannel channel)
        : IRequestHandler<Request, Response?>
    {
        public async Task<Response?> Handle(Request request, CancellationToken cancellationToken)
        {
            // Check if there's already an in-progress sync for this provider
            var existingJob = await db.SyncJobs
                .Where(j => j.ProviderName == request.Provider &&
                           (j.Status == SyncJobStatus.Pending || j.Status == SyncJobStatus.InProgress))
                .FirstOrDefaultAsync(cancellationToken);

            if (existingJob != null)
            {
                return new Response(
                    existingJob.Id,
                    existingJob.Status.ToString(),
                    "A sync job is already in progress for this provider"
                );
            }

            // Create new sync job
            var job = new SyncJob
            {
                Id = Guid.NewGuid(),
                ProviderName = request.Provider,
                StartedAt = DateTimeOffset.UtcNow,
                Status = SyncJobStatus.Pending,
                TotalItems = 0,
                ProcessedItems = 0
            };

            db.SyncJobs.Add(job);
            await db.SaveChangesAsync(cancellationToken);

            // Enqueue the sync request
            await channel.Writer.WriteAsync(
                new SyncRequest(job.Id, request.Provider),
                cancellationToken);

            return new Response(
                job.Id,
                job.Status.ToString(),
                "Sync job queued successfully"
            );
        }
    }
}
