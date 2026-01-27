using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Tags;

public static class RenameTag
{
    public record Request(Guid Id, string NewName) : IRequest<Response>;
    
    public record Response(Guid Id, string Name, DateTime UpdatedAt);

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (request.Id == Guid.Empty)
            {
                error = "Tag ID is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.NewName))
            {
                error = "Tag name is required";
                return false;
            }

            if (request.NewName.Length > 100)
            {
                error = "Tag name must be 100 characters or less";
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

            var normalizedName = request.NewName.Trim();

            // Check if another tag already has this name (case-insensitive)
            var existingTag = await db.Tags
                .FirstOrDefaultAsync(t => t.Id != request.Id && t.Name.ToLower() == normalizedName.ToLower(), cancellationToken);

            if (existingTag != null)
            {
                throw new InvalidOperationException($"Tag '{normalizedName}' already exists");
            }

            tag.Name = normalizedName;
            tag.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);

            return new Response(tag.Id, tag.Name, tag.UpdatedAt);
        }
    }
}
