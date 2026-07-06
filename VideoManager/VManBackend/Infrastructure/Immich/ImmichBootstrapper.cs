using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace VManBackend.Infrastructure.Immich;

/// <summary>
/// Obtains a working Immich API key by driving Immich's own admin-sign-up / login / api-key
/// endpoints, so a fresh Immich instance doesn't require any manual setup through its UI.
/// </summary>
public static class ImmichBootstrapper
{
    public static async Task<string> EnsureApiKeyAsync(ImmichBootstrapOptions options, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(options.CacheFilePath) && File.Exists(options.CacheFilePath))
        {
            var cached = (await File.ReadAllTextAsync(options.CacheFilePath, cancellationToken)).Trim();
            if (!string.IsNullOrWhiteSpace(cached))
            {
                Console.WriteLine("✅ Reusing cached Immich API key.");
                return cached;
            }
        }

        using var http = new HttpClient { BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/") };

        await WaitForImmichAsync(http, options, cancellationToken);
        await TryCreateAdminAsync(http, options, cancellationToken);
        var accessToken = await LoginAsync(http, options, cancellationToken);
        var apiKey = await CreateApiKeyAsync(http, accessToken, options, cancellationToken);

        if (!string.IsNullOrWhiteSpace(options.CacheFilePath))
        {
            var directory = Path.GetDirectoryName(options.CacheFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            await File.WriteAllTextAsync(options.CacheFilePath, apiKey, cancellationToken);
        }

        return apiKey;
    }

    private static async Task WaitForImmichAsync(HttpClient http, ImmichBootstrapOptions options, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + options.WaitTimeout;
        Exception? lastError = null;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var response = await http.GetAsync("server/ping", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                lastError = ex;
            }

            await Task.Delay(options.RetryDelay, cancellationToken);
        }

        throw new InvalidOperationException(
            $"Immich did not become ready at {http.BaseAddress} within {options.WaitTimeout}.", lastError);
    }

    private static async Task TryCreateAdminAsync(HttpClient http, ImmichBootstrapOptions options, CancellationToken cancellationToken)
    {
        var response = await http.PostAsJsonAsync("auth/admin-sign-up", new
        {
            email = options.AdminEmail,
            password = options.AdminPassword,
            name = options.AdminName
        }, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"✅ Created Immich admin account: {options.AdminEmail}");
            return;
        }

        // admin-sign-up is a one-time operation per Immich database - Immich responds with
        // 400 Bad Request ("The server already has an admin") on every subsequent call, which
        // is the expected steady state once bootstrap has run once.
        if (response.StatusCode != HttpStatusCode.BadRequest)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Immich admin-sign-up failed ({(int)response.StatusCode}): {body}");
        }
    }

    private static async Task<string> LoginAsync(HttpClient http, ImmichBootstrapOptions options, CancellationToken cancellationToken)
    {
        var response = await http.PostAsJsonAsync("auth/login", new
        {
            email = options.AdminEmail,
            password = options.AdminPassword
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Immich admin login failed ({(int)response.StatusCode}): {body}. " +
                "If Immich already has a different admin account, set IMMICH_ADMIN_PASSWORD " +
                "(and Immich:Bootstrap:AdminEmail) to match it, or supply IMMICH_API_KEY directly.");
        }

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
        if (!payload.TryGetProperty("accessToken", out var tokenProp) || tokenProp.GetString() is not { Length: > 0 } accessToken)
        {
            throw new InvalidOperationException("Immich login response did not include an access token.");
        }

        return accessToken;
    }

    private static async Task<string> CreateApiKeyAsync(HttpClient http, string accessToken, ImmichBootstrapOptions options, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api-keys")
        {
            Content = JsonContent.Create(new
            {
                name = options.ApiKeyName,
                permissions = new[] { "all" }
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Immich API key creation failed ({(int)response.StatusCode}): {body}");
        }

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
        if (!payload.TryGetProperty("secret", out var secretProp) || secretProp.GetString() is not { Length: > 0 } secret)
        {
            throw new InvalidOperationException("Immich API key creation response did not include a secret.");
        }

        Console.WriteLine($"✅ Created Immich API key: {options.ApiKeyName}");
        return secret;
    }
}

public class ImmichBootstrapOptions
{
    public required string BaseUrl { get; init; }
    public required string AdminEmail { get; init; }
    public required string AdminPassword { get; init; }
    public string AdminName { get; init; } = "VMan Bootstrap Admin";
    public string ApiKeyName { get; init; } = "vman-backend";
    public string? CacheFilePath { get; init; }
    public TimeSpan WaitTimeout { get; init; } = TimeSpan.FromMinutes(2);
    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromSeconds(2);
}
