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
            // Build filter for media provider
            var filter = new MediaItemFilter
            {
                Type = request.Type,
                IsFavorite = request.IsFavorite,
                SortBy = request.SortBy ?? "createdAt",
                Descending = request.SortDescending,
                Page = request.Page,
                PageSize = request.PageSize
            };

            // Fetch items from provider
            var result = await mediaProvider.GetItemsAsync(filter, cancellationToken);

            // Get all tags for these items (batch fetch for performance)
            var itemIds = result.Items.Select(i => i.Id).ToList();
            var providerName = request.Provider ?? "immich"; // Default to immich for now

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
            var items = result.Items.Select(item => new ItemDto(
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

            return new Response(items, result.TotalCount, result.Page, result.PageSize);
        }
    }
}
