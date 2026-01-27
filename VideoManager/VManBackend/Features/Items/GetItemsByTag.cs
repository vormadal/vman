using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Infrastructure.Providers;
using VManBackend.Mediator;

namespace VManBackend.Features.Items;

public static class GetItemsByTag
{
    public record Request(Guid TagId, int Page = 1, int PageSize = 50) : IRequest<Response>;
    
    public record ItemDto(
        string Provider,
        string Id,
        string Name,
        MediaType Type,
        DateTime CreatedAt,
        string? ThumbnailUrl,
        string? PreviewUrl,
        bool IsFavorite
    );
    
    public record Response(List<ItemDto> Items, int TotalCount, int Page, int PageSize);

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (request.TagId == Guid.Empty)
            {
                error = "Tag ID is required";
                return false;
            }

            if (request.Page < 1)
            {
                error = "Page must be greater than 0";
                return false;
            }

            if (request.PageSize < 1 || request.PageSize > 100)
            {
                error = "PageSize must be between 1 and 100";
                return false;
            }

            error = null;
            return true;
        }
    }

    public class Handler(ApplicationDbContext db, IMediaProvider mediaProvider) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            // Verify tag exists
            var tag = await db.Tags.FindAsync([request.TagId], cancellationToken);
            if (tag == null)
            {
                throw new InvalidOperationException($"Tag with ID '{request.TagId}' not found");
            }

            // Get all ItemTags for this tag
            var itemTags = await db.ItemTags
                .Where(it => it.TagId == request.TagId)
                .ToListAsync(cancellationToken);

            var totalCount = itemTags.Count;

            // Apply pagination
            var pagedItemTags = itemTags
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Fetch items from provider (currently only supports single provider per request)
            // Group by provider to batch fetch
            var itemsByProvider = pagedItemTags.GroupBy(it => it.ProviderName);
            
            var items = new List<ItemDto>();
            
            foreach (var providerGroup in itemsByProvider)
            {
                var providerName = providerGroup.Key;
                var itemIds = providerGroup.Select(it => it.ProviderItemId).ToList();

                // Fetch each item (could be optimized with batch fetch if provider supports it)
                foreach (var itemId in itemIds)
                {
                    try
                    {
                        var mediaItem = await mediaProvider.GetItemByIdAsync(itemId, cancellationToken);
                        if (mediaItem != null)
                        {
                            items.Add(new ItemDto(
                                providerName,
                                mediaItem.Id,
                                mediaItem.OriginalFileName,
                                mediaItem.Type,
                                mediaItem.CreatedAt,
                                mediaItem.ThumbnailUrl,
                                mediaItem.PreviewUrl,
                                mediaItem.IsFavorite
                            ));
                        }
                    }
                    catch
                    {
                        // Skip items that can't be fetched (might have been deleted from provider)
                        continue;
                    }
                }
            }

            return new Response(items, totalCount, request.Page, request.PageSize);
        }
    }
}
