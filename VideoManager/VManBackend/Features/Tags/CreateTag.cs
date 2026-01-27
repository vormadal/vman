using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Tags;

public static class CreateTag
{
    public record Request(string Name) : IRequest<Response>;
    
    public record Response(Guid Id, string Name, DateTime CreatedAt);

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                error = "Tag name is required";
                return false;
            }

            if (request.Name.Length > 100)
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
            var normalizedName = request.Name.Trim();

            // Check if tag already exists (case-insensitive)
            var existingTag = await db.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == normalizedName.ToLower(), cancellationToken);

            if (existingTag != null)
            {
                throw new InvalidOperationException($"Tag '{normalizedName}' already exists");
            }

            var tag = new Tag
            {
                Id = Guid.NewGuid(),
                Name = normalizedName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Tags.Add(tag);
            await db.SaveChangesAsync(cancellationToken);

            return new Response(tag.Id, tag.Name, tag.CreatedAt);
        }
    }
}
