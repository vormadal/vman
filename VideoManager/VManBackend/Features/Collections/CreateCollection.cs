using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Collections;

public static class CreateCollection
{
    public record Request(string Name, string? Description) : IRequest<Response>;
    
    public record Response(Guid Id, string Name, string? Description, DateTime CreatedAt);

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                error = "Collection name is required";
                return false;
            }

            if (request.Name.Length > 200)
            {
                error = "Collection name must be 200 characters or less";
                return false;
            }

            if (request.Description?.Length > 1000)
            {
                error = "Collection description must be 1000 characters or less";
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

            var collection = new Collection
            {
                Id = Guid.NewGuid(),
                Name = normalizedName,
                Description = request.Description?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Collections.Add(collection);
            await db.SaveChangesAsync(cancellationToken);

            return new Response(collection.Id, collection.Name, collection.Description, collection.CreatedAt);
        }
    }
}
