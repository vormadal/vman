using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Infrastructure.Authentication;
using VManBackend.Mediator;

namespace VManBackend.Features.Authentication;

public static class Register
{
    public record Request(
        string Email,
        string Password,
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
        string LastName
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

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                error = "Password is required";
                return false;
            }

            if (request.Password.Length < 8)
            {
                error = "Password must be at least 8 characters";
                return false;
            }

            if (request.Password.Length > 100)
            {
                error = "Password must be less than 100 characters";
                return false;
            }

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

    public class Handler(ApplicationDbContext db, IJwtService jwtService) : IRequestHandler<Request, Response?>
    {
        public async Task<Response?> Handle(Request request, CancellationToken cancellationToken)
        {
            // Check if user already exists
            var existingUser = await db.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

            if (existingUser != null)
            {
                return null; // Email already in use
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 4);

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.ToLower(),
                PasswordHash = passwordHash,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);

            // Generate JWT token
            var token = jwtService.GenerateToken(user);

            // Return user and token (auto-login)
            return new Response(
                new UserDto(user.Id, user.Email, user.FirstName, user.LastName),
                token,
                token // Using same token as refresh for now
            );
        }
    }
}
