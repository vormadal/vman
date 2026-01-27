using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Tags;

public static class DeleteTag
{
    public record Request(Guid Id) : IRequest<Response>;
    
    public record Response(bool Success);

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (request.Id == Guid.Empty)
            {
                error = "Tag ID is required";
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
            var tag = await db.Tags.FindAsync([request.Id], cancellationToken);
            if (tag == null)
            {
                throw new InvalidOperationException($"Tag with ID '{request.Id}' not found");
            }

            // Delete associated ItemTags first (cascade delete)
            var itemTags = await db.ItemTags
                .Where(it => it.TagId == request.Id)
                .ToListAsync(cancellationToken);
            
            db.ItemTags.RemoveRange(itemTags);
            db.Tags.Remove(tag);

            await db.SaveChangesAsync(cancellationToken);

            return new Response(true);
        }
    }
}
