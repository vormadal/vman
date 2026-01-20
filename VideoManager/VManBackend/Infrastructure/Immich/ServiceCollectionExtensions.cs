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
            
            var authProvider = new ApiKeyAuthenticationProvider(
                "x-api-key", 
                options.ApiKey, 
                ApiKeyAuthenticationProvider.KeyLocation.Header);
            
            var adapter = new HttpClientRequestAdapter(authProvider)
            {
                BaseUrl = options.BaseUrl
            };
            
            var client = new ImmichClient(adapter);
            return new ImmichService(client);
        });

        return services;
    }
}
