using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Mediator;

namespace VManBackend.Features.Collections;

public static class GetCollections
{
    public record Request(int Page = 1, int PageSize = 50) : IRequest<Response>;
    
    public record CollectionDto(Guid Id, string Name, string? Description, int ItemCount, DateTime CreatedAt, DateTime UpdatedAt);
    
    public record Response(List<CollectionDto> Collections, int TotalCount, int Page, int PageSize);

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
            var query = db.Collections.AsQueryable();

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and sort by updated date (most recent first)
            var collections = await query
                .GroupJoin(
                    db.CollectionItems,
                    c => c.Id,
                    ci => ci.CollectionId,
                    (c, items) => new
                    {
                        Collection = c,
                        ItemCount = items.Count()
                    })
                .OrderByDescending(c => c.Collection.UpdatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var collectionDtos = collections.Select(c => new CollectionDto(
                c.Collection.Id,
                c.Collection.Name,
                c.Collection.Description,
                c.ItemCount,
                c.Collection.CreatedAt,
                c.Collection.UpdatedAt
            )).ToList();

            return new Response(collectionDtos, totalCount, request.Page, request.PageSize);
        }
    }
}
