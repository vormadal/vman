using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Mediator;

namespace VManBackend.Features.People;

public static class GetPeople
{
    public record Request(string? Search = null, int Page = 1, int PageSize = 50) : IRequest<Response>;
    
    public record PersonDto(Guid Id, string Name, string? BirthDate, bool IsFavorite, bool IsHidden, int ItemCount, DateTime UpdatedAt);
    
    public record Response(List<PersonDto> People, int TotalCount, int Page, int PageSize);

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
            var query = db.People.AsQueryable();

            // Apply search filter (case-insensitive contains)
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchLower));
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Get person IDs for this page
            var pagedPeople = await query
                .OrderBy(p => p.Name)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Batch fetch item counts for all people on this page
            var personIds = pagedPeople.Select(p => p.Id).ToList();
            var itemCounts = await db.ItemPeople
                .Where(ip => personIds.Contains(ip.PersonId))
                .GroupBy(ip => ip.PersonId)
                .Select(g => new { PersonId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PersonId, x => x.Count, cancellationToken);

            var peopleDtos = pagedPeople.Select(p => new PersonDto(
                p.Id,
                p.Name,
                p.BirthDate?.ToString("yyyy-MM-dd"),
                p.IsFavorite,
                p.IsHidden,
                itemCounts.GetValueOrDefault(p.Id, 0),
                p.UpdatedAt.UtcDateTime
            )).ToList();

            return new Response(peopleDtos, totalCount, request.Page, request.PageSize);
        }
    }
}
