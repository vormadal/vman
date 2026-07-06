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

    public class Handler(ApplicationDbContext db, IJwtService jwtService, IRefreshTokenService refreshTokenService) : IRequestHandler<Request, Response?>
    {
        public async Task<Response?> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

            if (user == null || user.IsBlocked)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            user.LastLoginAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            var accessToken = jwtService.GenerateToken(user);
            var refreshToken = await refreshTokenService.CreateRefreshTokenAsync(user.Id, cancellationToken);

            return new Response(
                new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString()),
                accessToken,
                refreshToken,
                user.IsProfileComplete
            );
        }
    }
}
