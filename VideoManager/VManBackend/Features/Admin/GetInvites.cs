using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Admin;

public static class GetInvites
{
    public record Request() : IRequest<Response?>;

    public record Response(
        List<InviteDto> Invites
    );

    public record InviteDto(
        Guid Id,
        string Email,
        string Token,
        DateTime CreatedAt,
        DateTime? UsedAt,
        DateTime ExpiresAt,
        bool IsExpired,
        bool IsUsed
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
            
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return null; // Invalid user ID
            }

            // Verify current user is admin
            var currentUser = await db.Users.FindAsync([userId], cancellationToken: cancellationToken);
            if (currentUser == null || currentUser.Role != UserRole.Admin)
            {
                return null; // Not authorized
            }

            // Get all invites
            var invites = await db.UserInvites
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new InviteDto(
                    i.Id,
                    i.Email,
                    i.Token,
                    i.CreatedAt,
                    i.UsedAt,
                    i.ExpiresAt,
                    i.ExpiresAt < DateTime.UtcNow,
                    i.UsedAt != null
                ))
                .ToListAsync(cancellationToken);

            return new Response(invites);
        }
    }
}
