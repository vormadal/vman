using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoManager.Common.Data;
using VideoManager.Infrastructure.Authentication;
using BC = BCrypt.Net.BCrypt;

namespace VideoManager.Features.Auth;

public static class Login
{
    public record Request(string Email, string Password);
    public record Response(string Token, string Email, string FirstName, string LastName);

    [ApiController]
    [Route("api/auth")]
    public class Controller : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public Controller(ApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<Response>> Handle([FromBody] Request request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BC.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            return Ok(new Response(token, user.Email, user.FirstName, user.LastName));
        }
    }
}
