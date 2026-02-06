using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Infrastructure.Authentication;
using VManBackend.Mediator;
using System.Security.Claims;

namespace VManBackend.Features.Authentication;

public static class CompleteProfile
{
    public record Request(
        string FirstName,
        string LastName
    ) : IRequest<Response?>;

    public record Response(
        UserDto User,
        string AccessToken,
        string RefreshToken
    );

    public record UserDto(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        string Role
    );

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                error = "First name is required";
                return false;
            }

            if (request.FirstName.Length > 100)
            {
                error = "First name must be less than 100 characters";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                error = "Last name is required";
                return false;
            }

            if (request.LastName.Length > 100)
            {
                error = "Last name must be less than 100 characters";
                return false;
            }

            error = null;
            return true;
        }
    }

    public class Handler(ApplicationDbContext db, IJwtService jwtService, IHttpContextAccessor httpContextAccessor) : IRequestHandler<Request, Response?>
    {
        public async Task<Response?> Handle(Request request, CancellationToken cancellationToken)
        {
            // Get user ID from JWT claims
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return null; // Not authenticated
            }

            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier) 
                ?? httpContext.User.FindFirst("sub");
            
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return null; // Invalid user ID
            }

            // Find user
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                return null; // User not found
            }

            // Update profile
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.IsProfileComplete = true;

            await db.SaveChangesAsync(cancellationToken);

            // Generate new JWT token with updated info
            var token = jwtService.GenerateToken(user);

            // Return updated user and token
            return new Response(
                new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString()),
                token,
                token // Using same token as refresh for now
            );
        }
    }
}
