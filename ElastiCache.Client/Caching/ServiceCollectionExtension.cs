using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ElastiCache.Client.Caching
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddTypedHttpClients<TClient, TImplementation, TSettings>(
    this IServiceCollection services)
    where TClient : class
    where TImplementation : class, TClient
    where TSettings : HttpSettings
        {            
            services.AddHttpClient<TClient, TImplementation>((provider, client) =>
            {
                var settings = provider.GetRequiredService<IOptions<TSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseApiUrl);
                // Additional configuration (headers, timeouts, etc.) can be done here
            });
            return services;
        }

    }
}
