using FluentAssertions;
using Microsoft.Extensions.Configuration;
using VManBackend.Common.Models;
using VManBackend.Infrastructure.Authentication;
using Xunit;

namespace VManBackend.Tests.Infrastructure.Authentication;

public class JwtServiceTests
{
    private static IConfiguration BuildConfiguration(int expirationMinutes = 60) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-min-32-characters-long",
                ["Jwt:Issuer"] = "VManBackend",
                ["Jwt:Audience"] = "VManBackend",
                ["Jwt:ExpirationMinutes"] = expirationMinutes.ToString()
            })
            .Build();

    private static User NewUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "user@example.com",
        PasswordHash = "hash",
        FirstName = "Jane",
        LastName = "Doe",
        Role = UserRole.User
    };

    [Fact]
    public void GenerateToken_ThenValidateToken_RoundTripsSuccessfully()
    {
        var service = new JwtService(BuildConfiguration());
        var user = NewUser();

        var token = service.GenerateToken(user);
        var principal = service.ValidateToken(token);

        principal.Should().NotBeNull();
        principal!.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value.Should().Be("User");
    }

    [Fact]
    public void ValidateToken_ReturnsNull_ForExpiredToken()
    {
        var service = new JwtService(BuildConfiguration(expirationMinutes: -1));
        var user = NewUser();
        var token = service.GenerateToken(user);

        var principal = service.ValidateToken(token);

        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_ReturnsNull_ForWrongIssuer()
    {
        var issuingService = new JwtService(BuildConfiguration());
        var token = issuingService.GenerateToken(NewUser());

        var configWithDifferentIssuer = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-min-32-characters-long",
                ["Jwt:Issuer"] = "SomeOtherIssuer",
                ["Jwt:Audience"] = "VManBackend"
            })
            .Build();
        var validatingService = new JwtService(configWithDifferentIssuer);

        var principal = validatingService.ValidateToken(token);

        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_ReturnsNull_ForGarbageToken()
    {
        var service = new JwtService(BuildConfiguration());

        var principal = service.ValidateToken("not-a-valid-jwt");

        principal.Should().BeNull();
    }

    [Fact]
    public void Constructor_Throws_WhenSecretKeyMissing()
    {
        var config = new ConfigurationBuilder().Build();

        var act = () => new JwtService(config);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_Throws_WhenSecretKeyBlank()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "   "
            })
            .Build();

        var act = () => new JwtService(config);

        act.Should().Throw<InvalidOperationException>();
    }
}
