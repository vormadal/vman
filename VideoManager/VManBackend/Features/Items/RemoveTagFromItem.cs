using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Mediator;

namespace VManBackend.Features.Items;

public static class RemoveTagFromItem
{
    public record Request(string Provider, string ItemId, Guid TagId) : IRequest<Response>;
    
    public record Response(bool Success);

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (string.IsNullOrWhiteSpace(request.Provider))
            {
                error = "Provider is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.ItemId))
            {
                error = "Item ID is required";
                return false;
            }

            if (request.TagId == Guid.Empty)
            {
                error = "Tag ID is required";
                return false;
            }

            error = null;
            return true;
        }
    }

    public class Handler(ApplicationDbContext db) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var itemTag = await db.ItemTags
                .FirstOrDefaultAsync(it => 
                    it.TagId == request.TagId && 
                    it.ProviderName == request.Provider && 
                    it.ProviderItemId == request.ItemId, 
                    cancellationToken);

            if (itemTag == null)
            {
                // Already not tagged, return success (idempotent)
                return new Response(true);
            }

            db.ItemTags.Remove(itemTag);
            await db.SaveChangesAsync(cancellationToken);

            return new Response(true);
        }
    }
}
