using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Mediator;

namespace VManBackend.Features.Sync;

public static class GetSyncStatus
{
    public record Request(Guid? JobId = null, string Provider = "immich") : IRequest<Response?>;

    public record Response(
        Guid JobId,
        string ProviderName,
        string Status,
        DateTimeOffset StartedAt,
        DateTimeOffset? CompletedAt,
        int TotalItems,
        int ProcessedItems,
        string? ErrorMessage
    );

    public static class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (string.IsNullOrWhiteSpace(request.Provider))
            {
                error = "Provider is required";
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
            var query = db.SyncJobs.AsQueryable();

            if (request.JobId.HasValue)
            {
                query = query.Where(j => j.Id == request.JobId.Value);
            }
            else
            {
                query = query.Where(j => j.ProviderName == request.Provider);
            }

            var job = await query
                .OrderByDescending(j => j.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (job == null)
            {
                return null;
            }

            return new Response(
                job.Id,
                job.ProviderName,
                job.Status.ToString(),
                job.StartedAt,
                job.CompletedAt,
                job.TotalItems,
                job.ProcessedItems,
                job.ErrorMessage
            );
        }
    }
}
