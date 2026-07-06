using VManBackend.Infrastructure.Authentication;
using VManBackend.Mediator;

namespace VManBackend.Features.Authentication;

public static class Logout
{
    public record Request(string RefreshToken) : IRequest<bool>;

    public class Handler(IRefreshTokenService refreshTokenService) : IRequestHandler<Request, bool>
    {
        public async Task<bool> Handle(Request request, CancellationToken cancellationToken)
        {
            await refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
            return true;
        }
    }
}
