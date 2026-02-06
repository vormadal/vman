using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;

namespace VManBackend.Features.Admin;

public static class GetUsers
{
    public record Request() : IRequest<Response?>;

    public record Response(
        List<UserDto> Users
    );

    public record UserDto(
        Guid Id,
        string Email,
        string? FirstName,
        string? LastName,
        string Role,
        bool IsBlocked,
        bool IsProfileComplete,
        DateTime CreatedAt,
        DateTime? LastLoginAt
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

            // Get all users
            var users = await db.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserDto(
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.Role.ToString(),
                    u.IsBlocked,
                    u.IsProfileComplete,
                    u.CreatedAt,
                    u.LastLoginAt
                ))
                .ToListAsync(cancellationToken);

            return new Response(users);
        }
    }
}
