using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Tags;

public static class GetTagById
{
    public record Request(Guid Id) : IRequest<Response>;
    
    public record Response(Guid Id, string Name, int ItemCount, DateTime CreatedAt, DateTime UpdatedAt);

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

            var itemCount = await db.ItemTags
                .CountAsync(it => it.TagId == request.Id, cancellationToken);

            return new Response(tag.Id, tag.Name, itemCount, tag.CreatedAt, tag.UpdatedAt);
        }
    }
}
