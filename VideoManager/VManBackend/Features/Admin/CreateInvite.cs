using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Mediator;
using System.Security.Cryptography;

namespace VManBackend.Features.Admin;

public static class CreateInvite
{
    public record Request(
        string Email
    ) : IRequest<Response?>;

    public record Response(
        Guid Id,
        string Email,
        string Token,
        string InviteUrl,
        DateTime ExpiresAt
    );

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                error = "Email is required";
                return false;
            }

            if (request.Email.Length > 255)
            {
                error = "Email must be less than 255 characters";
                return false;
            }

            if (!IsValidEmail(request.Email))
            {
                error = "Email format is invalid";
                return false;
            }

            error = null;
            return true;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
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

            // Check if email already has a user
            var existingUser = await db.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

            if (existingUser != null)
            {
                return null; // Email already in use
            }

            // Generate secure token
            var token = GenerateSecureToken();

            // Create invite
            var invite = new UserInvite
            {
                Id = Guid.NewGuid(),
                Email = request.Email.ToLower(),
                Token = token,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7) // Invite expires in 7 days
            };

            db.UserInvites.Add(invite);
            await db.SaveChangesAsync(cancellationToken);

            // Generate invite URL (you'll need to configure the base URL)
            var baseUrl = httpContext.Request.Scheme + "://" + httpContext.Request.Host;
            var inviteUrl = $"{baseUrl}/accept-invite?token={token}";

            return new Response(
                invite.Id,
                invite.Email,
                invite.Token,
                inviteUrl,
                invite.ExpiresAt
            );
        }

        private static string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}
