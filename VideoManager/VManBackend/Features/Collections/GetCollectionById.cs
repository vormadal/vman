using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Mediator;
using VManBackend.Infrastructure.Providers;

namespace VManBackend.Features.Collections;

public static class GetCollectionById
{
    public record Request(Guid Id) : IRequest<Response>;
    
    public record CollectionItemDto(Guid Id, string ProviderName, string ProviderItemId, int Order, DateTime CreatedAt);
    
    public record Response(Guid Id, string Name, string? Description, List<CollectionItemDto> Items, DateTime CreatedAt, DateTime UpdatedAt);

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (request.Id == Guid.Empty)
            {
                error = "Collection ID is required";
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
            var collection = await db.Collections
                .Include(c => c.CollectionItems)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (collection == null)
            {
                throw new InvalidOperationException($"Collection with ID {request.Id} not found");
            }

            var items = collection.CollectionItems
                .OrderBy(ci => ci.Order)
                .Select(ci => new CollectionItemDto(
                    ci.Id,
                    ci.ProviderName,
                    ci.ProviderItemId,
                    ci.Order,
                    ci.CreatedAt
                ))
                .ToList();

            return new Response(
                collection.Id,
                collection.Name,
                collection.Description,
                items,
                collection.CreatedAt,
                collection.UpdatedAt
            );
        }
    }
}
