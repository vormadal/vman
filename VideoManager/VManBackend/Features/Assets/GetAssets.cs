using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Assets;

public static class GetAssets
{
    public record Request(
        AssetType? AssetType = null,
        int Page = 1,
        int PageSize = 50,
        string? SortBy = "CreatedAt",
        bool Descending = true
    ) : IRequest<Response>;

    public record Response(
        List<AssetDto> Assets,
        int TotalCount,
        int Page,
        int PageSize
    );

    public record AssetDto(
        Guid Id,
        string OriginalFileName,
        string OriginalPath,
        AssetType AssetType,
        DateTimeOffset CreatedAt,
        DateTimeOffset? FileCreatedAt,
        string? Duration,
        int? Width,
        int? Height,
        string? Description,
        bool IsFavorite,
        bool IsArchived,
        double? Rating
    );

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

    public class Handler(ApplicationDbContext db) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var query = db.ImmichAssets.AsQueryable();

            if (request.AssetType.HasValue)
            {
                query = query.Where(a => a.AssetType == request.AssetType.Value);
            }

            query = request.SortBy?.ToLower() switch
            {
                "filename" => request.Descending
                    ? query.OrderByDescending(a => a.OriginalFileName)
                    : query.OrderBy(a => a.OriginalFileName),
                "filecreatedat" => request.Descending
                    ? query.OrderByDescending(a => a.FileCreatedAt)
                    : query.OrderBy(a => a.FileCreatedAt),
                "rating" => request.Descending
                    ? query.OrderByDescending(a => a.Rating)
                    : query.OrderBy(a => a.Rating),
                _ => request.Descending
                    ? query.OrderByDescending(a => a.CreatedAt)
                    : query.OrderBy(a => a.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var assets = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AssetDto(
                    a.Id,
                    a.OriginalFileName,
                    a.OriginalPath,
                    a.AssetType,
                    a.CreatedAt,
                    a.FileCreatedAt,
                    a.Duration,
                    a.Width,
                    a.Height,
                    a.Description,
                    a.IsFavorite,
                    a.IsArchived,
                    a.Rating
                ))
                .ToListAsync(cancellationToken);

            return new Response(assets, totalCount, request.Page, request.PageSize);
        }
    }
}
