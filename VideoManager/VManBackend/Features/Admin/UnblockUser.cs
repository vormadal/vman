using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Admin;

public static class UnblockUser
{
    public record Request(
        Guid UserId
    ) : IRequest<Response?>;

    public record Response(
        bool Success
    );

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

            // Find target user
            var targetUser = await db.Users.FindAsync([request.UserId], cancellationToken: cancellationToken);
            if (targetUser == null)
            {
                return null; // User not found
            }

            // Unblock user
            targetUser.IsBlocked = false;
            await db.SaveChangesAsync(cancellationToken);

            return new Response(true);
        }
    }
}
