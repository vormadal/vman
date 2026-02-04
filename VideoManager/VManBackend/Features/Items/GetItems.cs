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
        bool? Untagged = null,
        Guid? TagId = null,
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
        DateTimeOffset CreatedAt,
        string ThumbnailUrl,
        string PreviewUrl,
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

            if (request.Untagged == true && request.TagId.HasValue)
            {
                error = "Cannot specify both Untagged and TagId filters";
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
            var providerName = request.Provider ?? "immich";

            // Start with base query from local database
            var query = db.Items
                .Where(i => i.ProviderName == providerName)
                .AsQueryable();

            // Apply type filter
            if (request.Type.HasValue)
            {
                query = query.Where(i => i.Type == request.Type.Value);
            }

            // Apply tag filters
            if (request.Untagged == true)
            {
                // Items with no tags
                query = query.Where(i => !db.ItemTags
                    .Any(it => it.ProviderName == i.ProviderName && it.ProviderItemId == i.ProviderItemId));
            }
            else if (request.TagId.HasValue)
            {
                // Items with a specific tag
                query = query.Where(i => db.ItemTags
                    .Any(it => it.ProviderName == i.ProviderName
                            && it.ProviderItemId == i.ProviderItemId
                            && it.TagId == request.TagId.Value));
            }

            // Apply sorting
            query = request.SortBy?.ToLowerInvariant() switch
            {
                "filename" => request.SortDescending
                    ? query.OrderByDescending(i => i.OriginalFileName)
                    : query.OrderBy(i => i.OriginalFileName),
                _ => request.SortDescending
                    ? query.OrderByDescending(i => i.CreatedAt)
                    : query.OrderBy(i => i.CreatedAt)
            };

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Batch fetch tags for all items on this page
            var itemIds = items.Select(i => i.ProviderItemId).ToList();
            var itemTagsDict = await db.ItemTags
                .Include(it => it.Tag)
                .Where(it => it.ProviderName == providerName && itemIds.Contains(it.ProviderItemId))
                .GroupBy(it => it.ProviderItemId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(it => new TagDto(it.Tag.Id, it.Tag.Name)).ToList(),
                    cancellationToken
                );

            // Map to DTOs with dynamically generated thumbnail URLs
            var itemDtos = items.Select(item => new ItemDto(
                item.ProviderName,
                item.ProviderItemId,
                item.OriginalFileName,
                item.Type,
                item.CreatedAt,
                $"/api/providers/{item.ProviderName}/items/{item.ProviderItemId}/thumbnail",
                $"/api/providers/{item.ProviderName}/items/{item.ProviderItemId}/preview",
                itemTagsDict.GetValueOrDefault(item.ProviderItemId, [])
            )).ToList();

            return new Response(itemDtos, totalCount, request.Page, request.PageSize);
        }
    }
}
