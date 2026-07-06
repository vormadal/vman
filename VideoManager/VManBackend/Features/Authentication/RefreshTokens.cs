using VManBackend.Infrastructure.Authentication;
using VManBackend.Mediator;

namespace VManBackend.Features.Authentication;

public static class RefreshTokens
{
    public record Request(string RefreshToken) : IRequest<Response?>;

    public record Response(
        string AccessToken,
        string RefreshToken
    );

    public class Handler(IJwtService jwtService, IRefreshTokenService refreshTokenService) : IRequestHandler<Request, Response?>
    {
        public async Task<Response?> Handle(Request request, CancellationToken cancellationToken)
        {
            var stored = await refreshTokenService.ValidateAndRotateAsync(request.RefreshToken, cancellationToken);
            if (stored == null)
                return null;

            var accessToken = jwtService.GenerateToken(stored.User);
            var newRefreshToken = await refreshTokenService.CreateRefreshTokenAsync(stored.UserId, cancellationToken);

            return new Response(accessToken, newRefreshToken);
        }
    }
}
