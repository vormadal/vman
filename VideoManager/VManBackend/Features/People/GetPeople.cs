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

            // Apply pagination and sort by name
            var people = await query
                .OrderBy(p => p.Name)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new
                {
                    Person = p,
                    ItemCount = db.ItemPeople.Count(ip => ip.PersonId == p.Id)
                })
                .ToListAsync(cancellationToken);

            var peopleDtos = people.Select(p => new PersonDto(
                p.Person.Id,
                p.Person.Name,
                p.Person.BirthDate?.ToString("yyyy-MM-dd"),
                p.Person.IsFavorite,
                p.Person.IsHidden,
                p.ItemCount,
                p.Person.UpdatedAt.DateTime
            )).ToList();

            return new Response(peopleDtos, totalCount, request.Page, request.PageSize);
        }
    }
}
