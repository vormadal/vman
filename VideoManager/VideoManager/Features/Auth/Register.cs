using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoManager.Common.Data;
using VideoManager.Common.Models;
using BC = BCrypt.Net.BCrypt;

namespace VideoManager.Features.Auth;

public static class Register
{
    public record Request(string Email, string Password, string FirstName, string LastName);
    public record Response(Guid Id, string Email, string FirstName, string LastName);

    [ApiController]
    [Route("api/auth")]
    public class Controller : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public Controller(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<Response>> Handle([FromBody] Request request)
        {
            // Validate email is unique
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email already registered" });
            }

            // Hash password
            var passwordHash = BC.HashPassword(request.Password);

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = passwordHash,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Handle),
                new Response(user.Id, user.Email, user.FirstName, user.LastName)
            );
        }
    }
}
