using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Infrastructure.Providers;
using VManBackend.Mediator;

namespace VManBackend.Features.Items;

public static class GetItems
{
    public record Request(
        string? Provider = null,
        MediaType? Type = null,
        bool? IsFavorite = null,
        bool? Untagged = null,  // Filter to show only untagged items
        string? SortBy = "createdAt",
        bool SortDescending = true,
        int Page = 1,
        int PageSize = 50
    ) : IRequest<Response>;
    
    public record TagDto(Guid Id, string Name);
    
    public record ItemDto(
        string Provider,
        string Id,
        string Name,
        MediaType Type,
        DateTime CreatedAt,
        string? ThumbnailUrl,
        string? PreviewUrl,
        bool IsFavorite,
        List<TagDto> Tags
    );
    
    public record Response(List<ItemDto> Items, int TotalCount, int Page, int PageSize);

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
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
            var providerName = request.Provider ?? "immich"; // Default to immich for now

            // If filtering by untagged, we need to fetch more items from provider
            // to account for items that will be filtered out
            var fetchFilter = new MediaItemFilter
            {
                Type = request.Type,
                IsFavorite = request.IsFavorite,
                SortBy = request.SortBy ?? "createdAt",
                Descending = request.SortDescending,
                Page = request.Untagged == true ? 1 : request.Page,  // Fetch from page 1 when filtering by untagged
                PageSize = request.Untagged == true ? 500 : request.PageSize  // Fetch more to compensate for filtering
            };

            // Fetch items from provider
            var result = await mediaProvider.GetItemsAsync(fetchFilter, cancellationToken);

            // Get all tags for these items (batch fetch for performance)
            var itemIds = result.Items.Select(i => i.Id).ToList();

            var itemTagsDict = await db.ItemTags
                .Include(it => it.Tag)
                .Where(it => it.ProviderName == providerName && itemIds.Contains(it.ProviderItemId))
                .GroupBy(it => it.ProviderItemId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(it => new TagDto(it.Tag.Id, it.Tag.Name)).ToList(),
                    cancellationToken
                );

            // Merge provider data with tags
            var allItems = result.Items.Select(item => new ItemDto(
                providerName,
                item.Id,
                item.OriginalFileName,
                item.Type,
                item.CreatedAt,
                item.ThumbnailUrl,
                item.PreviewUrl,
                item.IsFavorite,
                itemTagsDict.GetValueOrDefault(item.Id, new List<TagDto>())
            )).ToList();

            // Apply untagged filter if requested
            if (request.Untagged == true)
            {
                allItems = allItems.Where(item => item.Tags.Count == 0).ToList();
            }

            // Manual pagination when filtering by untagged
            var totalCount = request.Untagged == true ? allItems.Count : result.TotalCount;
            var items = request.Untagged == true
                ? allItems
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList()
                : allItems;

            return new Response(items, totalCount, request.Page, request.PageSize);
        }
    }
}
