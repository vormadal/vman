using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Mediator;

namespace VManBackend.Features.Collections;

public static class DeleteCollection
{
    public record Request(Guid Id) : IRequest<Response>;
    
    public record Response(bool Success);

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
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (collection == null)
            {
                throw new InvalidOperationException($"Collection with ID {request.Id} not found");
            }

            db.Collections.Remove(collection);
            await db.SaveChangesAsync(cancellationToken);

            return new Response(true);
        }
    }
}
