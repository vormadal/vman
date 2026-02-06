using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Admin;

public static class ChangeUserRole
{
    public record Request(
        Guid UserId,
        string Role
    ) : IRequest<Response?>;

    public record Response(
        bool Success
    );

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (!Enum.TryParse<UserRole>(request.Role, true, out _))
            {
                error = "Invalid role. Must be 'User' or 'Admin'";
                return false;
            }

            error = null;
            return true;
        }
    }

    public class Handler(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor) : IRequestHandler<Request, Response?>
    {
        public async Task<Response?> Handle(Request request, CancellationToken cancellationToken)
        {
            // Get current user ID from JWT claims
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return null; // Not authenticated
            }

            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                ?? httpContext.User.FindFirst("sub");
            
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
            {
                return null; // Invalid user ID
            }

            // Verify current user is admin
            var currentUser = await db.Users.FindAsync([currentUserId], cancellationToken: cancellationToken);
            if (currentUser == null || currentUser.Role != UserRole.Admin)
            {
                return null; // Not authorized
            }

            // Don't allow changing your own role
            if (request.UserId == currentUserId)
            {
                return null; // Cannot change your own role
            }

            // Find target user
            var targetUser = await db.Users.FindAsync([request.UserId], cancellationToken: cancellationToken);
            if (targetUser == null)
            {
                return null; // User not found
            }

            // Parse and set new role
            if (!Enum.TryParse<UserRole>(request.Role, true, out var newRole))
            {
                return null; // Invalid role
            }

            targetUser.Role = newRole;
            await db.SaveChangesAsync(cancellationToken);

            return new Response(true);
        }
    }
}
