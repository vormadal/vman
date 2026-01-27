using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Items;

public static class AddTagToItem
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
            // Verify tag exists
            var tag = await db.Tags.FindAsync([request.TagId], cancellationToken);
            if (tag == null)
            {
                throw new InvalidOperationException($"Tag with ID '{request.TagId}' not found");
            }

            // Check if this tag is already assigned to this item
            var existing = await db.ItemTags
                .FirstOrDefaultAsync(it => 
                    it.TagId == request.TagId && 
                    it.ProviderName == request.Provider && 
                    it.ProviderItemId == request.ItemId, 
                    cancellationToken);

            if (existing != null)
            {
                // Already tagged, return success (idempotent)
                return new Response(true);
            }

            // Create new ItemTag
            var itemTag = new ItemTag
            {
                Id = Guid.NewGuid(),
                TagId = request.TagId,
                ProviderName = request.Provider,
                ProviderItemId = request.ItemId,
                CreatedAt = DateTime.UtcNow
            };

            db.ItemTags.Add(itemTag);
            await db.SaveChangesAsync(cancellationToken);

            return new Response(true);
        }
    }
}
