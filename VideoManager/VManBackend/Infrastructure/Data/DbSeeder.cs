using Microsoft.EntityFrameworkCore;
using VManBackend.Common.Data;
using VManBackend.Common.Models;

namespace VManBackend.Infrastructure.Data;

public static class DbSeeder
{
    /// <summary>
    /// Seeds a test user for development and E2E testing.
    /// This method is idempotent - safe to call multiple times.
    /// </summary>
    public static async Task SeedTestUserAsync(ApplicationDbContext db, IConfiguration config)
    {
        var email = config["TestUser:Email"];
        var password = config["TestUser:Password"];
        var firstName = config["TestUser:FirstName"] ?? config["TEST_USER_FIRST_NAME"] ?? "Test";
        var lastName = config["TestUser:LastName"] ?? config["TEST_USER_LAST_NAME"] ?? "User";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("⚠️  Test user credentials not configured. Skipping test user seeding.");
            Console.WriteLine("   Set TEST_USER_EMAIL and TEST_USER_PASSWORD environment variables or add to appsettings.Development.json");
            return;
        }

        // Normalize email
        var normalizedEmail = email.ToLower();

        // Check if test user already exists
        var existingUser = await db.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (existingUser != null)
        {
            Console.WriteLine($"✅ Test user already exists: {email}");
            return;
        }

        // Create test user
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4);

        var testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };

        db.Users.Add(testUser);
        await db.SaveChangesAsync();

        Console.WriteLine($"✅ Test user created: {email}");
        Console.WriteLine($"   Name: {firstName} {lastName}");
        Console.WriteLine($"   Password: {new string('*', password.Length)} (from config)");
    }
}
