using Amazon.SecretsManager;
using Huron.AWS.Common.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Huron.AWS.SecretsManager.Core
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddSecretsManagerDependencies(this IServiceCollection services)
        {
            services.AddSingleton<AwsConfigOptions>();
            services.AddSingleton<AwsSecretsManagerConfigOptions>();
            services.AddSingleton<IAwsServiceClientFactory<IAmazonSecretsManager>, AwsSecretsManagerClientFactory>();
            services.AddScoped<IAwsSecretsManagerService, AwsSecretsManagerService>();
            return services;
        }
    }
}
