using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Sync;

public static class CancelSync
{
    public record Request(Guid JobId) : IRequest<Response?>;

    public record Response(Guid JobId, string Status, string Message);

    public static class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (request.JobId == Guid.Empty)
            {
                error = "JobId is required";
                return false;
            }

            error = null;
            return true;
        }
    }

    public class Handler(ApplicationDbContext db) : IRequestHandler<Request, Response?>
    {
        public async Task<Response?> Handle(Request request, CancellationToken cancellationToken)
        {
            var job = await db.SyncJobs.FindAsync([request.JobId], cancellationToken);

            if (job == null)
            {
                return null;
            }

            // Only allow cancellation of pending or in-progress jobs
            if (job.Status != SyncJobStatus.Pending && job.Status != SyncJobStatus.InProgress)
            {
                return new Response(
                    job.Id,
                    job.Status.ToString(),
                    $"Cannot cancel a job with status {job.Status}"
                );
            }

            job.Status = SyncJobStatus.Cancelled;
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.ErrorMessage = "Cancelled by user";

            await db.SaveChangesAsync(cancellationToken);

            return new Response(
                job.Id,
                job.Status.ToString(),
                "Sync job cancelled successfully"
            );
        }
    }
}
