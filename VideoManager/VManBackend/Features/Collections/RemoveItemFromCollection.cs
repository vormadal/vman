using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Mediator;

namespace VManBackend.Features.Collections;

public static class RemoveItemFromCollection
{
    public record Request(Guid CollectionId, Guid ItemId) : IRequest<Response>;
    
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

            if (request.ItemId == Guid.Empty)
            {
                error = "Item ID is required";
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
            var strategy = db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

                var collectionItem = await db.CollectionItems
                    .FirstOrDefaultAsync(ci =>
                        ci.CollectionId == request.CollectionId &&
                        ci.Id == request.ItemId,
                        cancellationToken);

                if (collectionItem == null)
                {
                    throw new InvalidOperationException("Item not found in collection");
                }

                var removedOrder = collectionItem.Order;

                db.CollectionItems.Remove(collectionItem);

                var itemsToReorder = await db.CollectionItems
                    .Where(ci => ci.CollectionId == request.CollectionId && ci.Order > removedOrder)
                    .ToListAsync(cancellationToken);

                foreach (var item in itemsToReorder)
                {
                    item.Order--;
                }

                var collection = await db.Collections.FindAsync([request.CollectionId], cancellationToken);
                if (collection != null)
                {
                    collection.UpdatedAt = DateTimeOffset.UtcNow;
                }

                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new Response(true);
            });
        }
    }
}
