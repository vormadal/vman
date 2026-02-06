using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Collections;

public static class AddItemToCollection
{
    public record Request(Guid CollectionId, string ProviderName, string ProviderItemId) : IRequest<Response>;
    
    public record Response(Guid Id, Guid CollectionId, string ProviderName, string ProviderItemId, int Order, DateTime CreatedAt);

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (request.CollectionId == Guid.Empty)
            {
                error = "Collection ID is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.ProviderName))
            {
                error = "Provider name is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.ProviderItemId))
            {
                error = "Provider item ID is required";
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

            // Check if item already exists in collection
            var existingItem = await db.CollectionItems
                .FirstOrDefaultAsync(ci => 
                    ci.CollectionId == request.CollectionId && 
                    ci.ProviderName == request.ProviderName && 
                    ci.ProviderItemId == request.ProviderItemId, 
                    cancellationToken);

            if (existingItem != null)
            {
                throw new InvalidOperationException("Item already exists in collection");
            }

            // Get the next order number (append to end)
            var maxOrder = await db.CollectionItems
                .Where(ci => ci.CollectionId == request.CollectionId)
                .MaxAsync(ci => (int?)ci.Order, cancellationToken) ?? -1;

            var collectionItem = new CollectionItem
            {
                Id = Guid.NewGuid(),
                CollectionId = request.CollectionId,
                ProviderName = request.ProviderName,
                ProviderItemId = request.ProviderItemId,
                Order = maxOrder + 1,
                CreatedAt = DateTime.UtcNow
            };

            db.CollectionItems.Add(collectionItem);
            
            // Update collection's UpdatedAt timestamp
            collection.UpdatedAt = DateTime.UtcNow;
            
            await db.SaveChangesAsync(cancellationToken);

            return new Response(
                collectionItem.Id,
                collectionItem.CollectionId,
                collectionItem.ProviderName,
                collectionItem.ProviderItemId,
                collectionItem.Order,
                collectionItem.CreatedAt
            );
        }
    }
}
