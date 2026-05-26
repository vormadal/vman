using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Infrastructure.Providers;
using VManBackend.Mediator;

namespace VManBackend.Features.Collections;

public static class BulkAddFilteredItemsToCollection
{
    public record Request(
        Guid CollectionId,
        string? Provider = null,
        MediaType? Type = null,
        Guid? TagId = null,
        Guid? PersonId = null
    ) : IRequest<Response>;

    public record Response(int AddedCount, int SkippedCount);

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (request.CollectionId == Guid.Empty)
            {
                error = "Collection ID is required";
                return false;
            }

            if (request.TagId == null && request.PersonId == null && request.Type == null)
            {
                error = "At least one filter (type, tagId, or personId) is required";
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
            var collection = await db.Collections.FindAsync([request.CollectionId], cancellationToken);
            if (collection == null)
            {
                throw new InvalidOperationException($"Collection with ID {request.CollectionId} not found");
            }

            var providerName = request.Provider ?? "immich";

            var query = db.Items
                .Where(i => i.ProviderName == providerName)
                .AsQueryable();

            if (request.Type.HasValue)
            {
                query = query.Where(i => i.Type == request.Type.Value);
            }

            if (request.TagId.HasValue)
            {
                query = query.Where(i => db.ItemTags
                    .Any(it => it.ProviderName == i.ProviderName
                            && it.ProviderItemId == i.ProviderItemId
                            && it.TagId == request.TagId.Value));
            }

            if (request.PersonId.HasValue)
            {
                query = query.Where(i => db.ItemPeople
                    .Any(ip => ip.ProviderName == i.ProviderName
                            && ip.ProviderItemId == i.ProviderItemId
                            && ip.PersonId == request.PersonId.Value));
            }

            var filteredItems = await query
                .Select(i => new { i.ProviderName, i.ProviderItemId })
                .ToListAsync(cancellationToken);

            var existingItemIds = await db.CollectionItems
                .Where(ci => ci.CollectionId == request.CollectionId && ci.ProviderName == providerName)
                .Select(ci => ci.ProviderItemId)
                .ToHashSetAsync(cancellationToken);

            var maxOrder = await db.CollectionItems
                .Where(ci => ci.CollectionId == request.CollectionId)
                .MaxAsync(ci => (int?)ci.Order, cancellationToken) ?? -1;

            var newItems = filteredItems
                .Where(i => !existingItemIds.Contains(i.ProviderItemId))
                .ToList();

            var order = maxOrder + 1;
            foreach (var item in newItems)
            {
                db.CollectionItems.Add(new CollectionItem
                {
                    Id = Guid.NewGuid(),
                    CollectionId = request.CollectionId,
                    ProviderName = item.ProviderName,
                    ProviderItemId = item.ProviderItemId,
                    Order = order++,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (newItems.Count > 0)
            {
                collection.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
            }

            return new Response(newItems.Count, filteredItems.Count - newItems.Count);
        }
    }
}
