using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Mediator;

namespace VManBackend.Features.Collections;

public static class UpdateCollectionItemOrder
{
    public record ItemOrderUpdate(Guid ItemId, int NewOrder);
    
    public record Request(Guid CollectionId, List<ItemOrderUpdate> Items) : IRequest<Response>;
    
    public record Response(bool Success);

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (request.CollectionId == Guid.Empty)
            {
                error = "Collection ID is required";
                return false;
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                error = "Items list is required";
                return false;
            }

            // Check for duplicate item IDs
            if (request.Items.Select(i => i.ItemId).Distinct().Count() != request.Items.Count)
            {
                error = "Duplicate item IDs in request";
                return false;
            }

            // Check for negative orders
            if (request.Items.Any(i => i.NewOrder < 0))
            {
                error = "Order values must be non-negative";
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
            // Check if collection exists
            var collection = await db.Collections.FindAsync([request.CollectionId], cancellationToken);
            if (collection == null)
            {
                throw new InvalidOperationException($"Collection with ID {request.CollectionId} not found");
            }

            // Get all items in the collection
            var collectionItems = await db.CollectionItems
                .Where(ci => ci.CollectionId == request.CollectionId)
                .ToListAsync(cancellationToken);

            // Update order for each item in the request
            foreach (var update in request.Items)
            {
                var item = collectionItems.FirstOrDefault(ci => ci.Id == update.ItemId);
                if (item == null)
                {
                    throw new InvalidOperationException($"Item with ID {update.ItemId} not found in collection");
                }

                item.Order = update.NewOrder;
            }

            // Update collection's UpdatedAt timestamp
            collection.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);

            return new Response(true);
        }
    }
}
