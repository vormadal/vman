using VManBackend.Common.Models;
using VManBackend.Infrastructure.Authentication;

namespace VManBackend.Tests.TestHelpers;

public class StubRefreshTokenService : IRefreshTokenService
{
    public Task<string> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(Guid.NewGuid().ToString());

    public Task<RefreshToken?> ValidateAndRotateAsync(string token, CancellationToken cancellationToken = default)
        => Task.FromResult<RefreshToken?>(null);

    public Task RevokeAsync(string token, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
