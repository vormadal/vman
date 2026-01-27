using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Tags;

public static class GetTags
{
    public record Request(string? Search = null, int Page = 1, int PageSize = 50) : IRequest<Response>;
    
    public record TagDto(Guid Id, string Name, int ItemCount, DateTime CreatedAt, DateTime UpdatedAt);
    
    public record Response(List<TagDto> Tags, int TotalCount, int Page, int PageSize);

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
            var query = db.Tags.AsQueryable();

            // Apply search filter (case-insensitive contains)
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(t => t.Name.ToLower().Contains(searchLower));
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and sort by name
            var tags = await query
                .OrderBy(t => t.Name)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new
                {
                    Tag = t,
                    ItemCount = db.ItemTags.Count(it => it.TagId == t.Id)
                })
                .ToListAsync(cancellationToken);

            var tagDtos = tags.Select(t => new TagDto(
                t.Tag.Id,
                t.Tag.Name,
                t.ItemCount,
                t.Tag.CreatedAt,
                t.Tag.UpdatedAt
            )).ToList();

            return new Response(tagDtos, totalCount, request.Page, request.PageSize);
        }
    }
}
