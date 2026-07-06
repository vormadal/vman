using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;
using VManBackend.Infrastructure.Authentication;
using VManBackend.Mediator;

namespace VManBackend.Features.Authentication;

public static class AcceptInvite
{
    public record Request(
        string Token,
        string Password
    ) : IRequest<Response?>;

    public record Response(
        UserDto User,
        string AccessToken,
        string RefreshToken
    );

    public record UserDto(
        Guid Id,
        string Email,
        string Role
    );

    public class Validator
    {
        public static bool Validate(Request request, out string? error)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                error = "Invite token is required";
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

            error = null;
            return true;
        }
    }

    public class Handler(ApplicationDbContext db, IJwtService jwtService, IRefreshTokenService refreshTokenService) : IRequestHandler<Request, Response?>
    {
        public async Task<Response?> Handle(Request request, CancellationToken cancellationToken)
        {
            var invite = await db.UserInvites
                .FirstOrDefaultAsync(i => i.Token == request.Token && i.UsedAt == null, cancellationToken);

            if (invite == null || invite.ExpiresAt < DateTime.UtcNow)
                return null;

            var existingUser = await db.Users
                .FirstOrDefaultAsync(u => u.Email == invite.Email.ToLower(), cancellationToken);

            if (existingUser != null)
                return null;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 4);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = invite.Email.ToLower(),
                PasswordHash = passwordHash,
                Role = UserRole.User,
                IsProfileComplete = false,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            invite.UsedAt = DateTime.UtcNow;

            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return null;
            }

            var accessToken = jwtService.GenerateToken(user);
            var refreshToken = await refreshTokenService.CreateRefreshTokenAsync(user.Id, cancellationToken);

            return new Response(
                new UserDto(user.Id, user.Email, user.Role.ToString()),
                accessToken,
                refreshToken
            );
        }
    }
}
