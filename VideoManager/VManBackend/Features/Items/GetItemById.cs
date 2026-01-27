using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Infrastructure.Providers;
using VManBackend.Mediator;

namespace VManBackend.Features.Items;

public static class GetItemById
{
    public record Request(string Provider, string ItemId) : IRequest<Response?>;
    
    public record TagDto(Guid Id, string Name);
    
    public record Response(
        string Provider,
        string Id,
        string Name,
        MediaType Type,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        string? Description,
        string? ThumbnailUrl,
        string? PreviewUrl,
        bool IsFavorite,
        long? FileSizeBytes,
        string? Duration,
        int? Width,
        int? Height,
        List<TagDto> Tags
    );

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

            error = null;
            return true;
        }
    }

    public class Handler(ApplicationDbContext db, IMediaProvider mediaProvider) : IRequestHandler<Request, Response?>
    {
        public async Task<Response?> Handle(Request request, CancellationToken cancellationToken)
        {
            // Fetch item from provider
            var item = await mediaProvider.GetItemByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
            {
                return null;
            }

            // Get tags for this item
            var tags = await db.ItemTags
                .Include(it => it.Tag)
                .Where(it => it.ProviderName == request.Provider && it.ProviderItemId == request.ItemId)
                .Select(it => new TagDto(it.Tag.Id, it.Tag.Name))
                .ToListAsync(cancellationToken);

            return new Response(
                request.Provider,
                item.Id,
                item.OriginalFileName,
                item.Type,
                item.CreatedAt,
                item.UpdatedAt,
                item.Description,
                item.ThumbnailUrl,
                item.PreviewUrl,
                item.IsFavorite,
                item.FileSizeBytes,
                item.Duration,
                item.Width,
                item.Height,
                tags
            );
        }
    }
}
