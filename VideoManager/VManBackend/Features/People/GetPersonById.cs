using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Mediator;

namespace VManBackend.Features.People;

public static class GetPersonById
{
    public record Request(Guid Id) : IRequest<Response?>;
    
    public record PersonDto(Guid Id, string Name, string? BirthDate, bool IsFavorite, bool IsHidden, int ItemCount, DateTime UpdatedAt);
    
    public record Response(PersonDto Person);

    public class Handler(ApplicationDbContext db) : IRequestHandler<Request, Response?>
    {
        public async Task<Response?> Handle(Request request, CancellationToken cancellationToken)
        {
            var person = await db.People
                .Where(p => p.Id == request.Id)
                .Select(p => new
                {
                    Person = p,
                    ItemCount = db.ItemPeople.Count(ip => ip.PersonId == p.Id)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (person == null)
                return null;

            var personDto = new PersonDto(
                person.Person.Id,
                person.Person.Name,
                person.Person.BirthDate?.ToString("yyyy-MM-dd"),
                person.Person.IsFavorite,
                person.Person.IsHidden,
                person.ItemCount,
                person.Person.UpdatedAt.DateTime
            );

            return new Response(personDto);
        }
    }
}
