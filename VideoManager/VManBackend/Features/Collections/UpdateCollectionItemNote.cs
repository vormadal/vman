using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Mediator;

namespace VManBackend.Features.Collections;

public static class UpdateCollectionItemNote
{
    public record Request(Guid CollectionId, Guid ItemId, string? Note) : IRequest<Response>;

    public record Response(Guid ItemId, string? Note);

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (request.CollectionId == Guid.Empty)
            {
                error = "Collection ID is required";
                return false;
            }

            if (request.ItemId == Guid.Empty)
            {
                error = "Item ID is required";
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
            var item = await db.CollectionItems
                .FirstOrDefaultAsync(ci =>
                    ci.Id == request.ItemId &&
                    ci.CollectionId == request.CollectionId,
                    cancellationToken);

            if (item == null)
            {
                throw new InvalidOperationException("Collection item not found");
            }

            item.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            await db.SaveChangesAsync(cancellationToken);

            return new Response(item.Id, item.Note);
        }
    }
}
