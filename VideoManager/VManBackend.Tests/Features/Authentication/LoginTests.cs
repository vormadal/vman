using FluentAssertions;
using Microsoft.Extensions.Configuration;
using VManBackend.Common.Models;
using VManBackend.Features.Authentication;
using VManBackend.Infrastructure.Authentication;
using VManBackend.Tests.TestHelpers;
using Xunit;

namespace VManBackend.Tests.Features.Authentication;

public class LoginTests
{
    private static IJwtService NewJwtService() => new JwtService(
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-min-32-characters-long"
            })
            .Build());

    private static User NewUser(string email, string password, bool isBlocked = false) => new()
    {
        Id = Guid.NewGuid(),
        Email = email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        IsBlocked = isBlocked
    };

    [Fact]
    public async Task Handle_ReturnsNull_WhenUserNotFound()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var handler = new Login.Handler(db, NewJwtService());

        var response = await handler.Handle(new Login.Request("missing@example.com", "password"), CancellationToken.None);

        response.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenUserIsBlocked()
    {
        await using var db = InMemoryDbContextFactory.Create();
        db.Users.Add(NewUser("blocked@example.com", "password", isBlocked: true));
        await db.SaveChangesAsync();
        var handler = new Login.Handler(db, NewJwtService());

        var response = await handler.Handle(new Login.Request("blocked@example.com", "password"), CancellationToken.None);

        response.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenPasswordIsWrong()
    {
        await using var db = InMemoryDbContextFactory.Create();
        db.Users.Add(NewUser("user@example.com", "correct-password"));
        await db.SaveChangesAsync();
        var handler = new Login.Handler(db, NewJwtService());

        var response = await handler.Handle(new Login.Request("user@example.com", "wrong-password"), CancellationToken.None);

        response.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ReturnsTokenAndUpdatesLastLogin_OnSuccess()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var user = NewUser("user@example.com", "correct-password");
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var handler = new Login.Handler(db, NewJwtService());

        var response = await handler.Handle(new Login.Request("user@example.com", "correct-password"), CancellationToken.None);

        response.Should().NotBeNull();
        response!.AccessToken.Should().NotBeNullOrEmpty();
        response.User.Email.Should().Be("user@example.com");
        user.LastLoginAt.Should().NotBeNull();
    }
}
