using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using VManBackend.Infrastructure.Immich.Generated;

namespace VManBackend.Infrastructure.Immich;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImmichClient(this IServiceCollection services, Action<ImmichOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddScoped<IImmichService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ImmichOptions>>().Value;

            // Use AnonymousAuthenticationProvider to bypass HTTPS requirement
            // API key is added via custom DelegatingHandler
            var authProvider = new AnonymousAuthenticationProvider();

            // Create HttpClient with custom handler that adds API key header
            var handler = new ImmichApiKeyHandler(options.ApiKey)
            {
                InnerHandler = new HttpClientHandler()
            };
            var httpClient = new HttpClient(handler);

            var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient)
            {
                BaseUrl = options.BaseUrl
            };

            var client = new ImmichClient(adapter);
            return new ImmichService(client);
        });

        return services;
    }
}
