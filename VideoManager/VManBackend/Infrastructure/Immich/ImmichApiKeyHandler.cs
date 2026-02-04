namespace VManBackend.Infrastructure.Immich;

public class ImmichApiKeyHandler : DelegatingHandler
{
    private readonly string _apiKey;

    public ImmichApiKeyHandler(string apiKey)
    {
        _apiKey = apiKey;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Add("x-api-key", _apiKey);
        return await base.SendAsync(request, cancellationToken);
    }
}
