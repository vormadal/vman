using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Infrastructure.Authentication;
using VManBackend.Mediator;

namespace VManBackend.Features.Authentication;

public static class Login
{
    public record Request(
        string Email,
        string Password
    ) : IRequest<Response?>;

    public record Response(
        UserDto User,
        string AccessToken,
        string RefreshToken,
        bool IsProfileComplete
    );

    public record UserDto(
        Guid Id,
        string Email,
        string? FirstName,
        string? LastName,
        string Role
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

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                error = "Password is required";
                return false;
            }

            error = null;
            return true;
        }
    }

    public class Handler(ApplicationDbContext db, IJwtService jwtService) : IRequestHandler<Request, Response?>
    {
        public async Task<Response?> Handle(Request request, CancellationToken cancellationToken)
        {
            // Find user by email
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

            if (user == null)
            {
                return null; // User not found
            }

            // Check if user is blocked
            if (user.IsBlocked)
            {
                return null; // User is blocked
            }

            // Verify password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return null; // Invalid password
            }

            // Update last login timestamp
            user.LastLoginAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            // Generate JWT token
            var token = jwtService.GenerateToken(user);

            // Return user and token
            return new Response(
                new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString()),
                token,
                token, // Using same token as refresh for now
                user.IsProfileComplete
            );
        }
    }
}
