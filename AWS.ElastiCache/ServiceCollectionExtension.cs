using Caching.Core;
using Huron.AWS.SecretsManager.Core;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using Serilog;
using Caching.AWS.ElastiCache;

namespace Caching.AWS.ElastiCache
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddRedisDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                string redisConfigurationEndPoint = GetRedisConfigurationEndPoint(services);

                string GetRedisConfigurationEndPoint(IServiceCollection services)
                {
                    string? currentEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    return currentEnvironment == "local"
                        ? Environment.GetEnvironmentVariable("LocalRedisConfigurationEndPoint")!
                        : GetRedisClusterConfigurationEndPoint(services)!;
                }
                string GetRedisClusterConfigurationEndPoint(IServiceCollection services)
                {
                    var redisClusterConfigurationEndpointSecretName = Environment.GetEnvironmentVariable("RedisClusterConfigurationEndpointSecretName");
                    var redisClusterConfigurationEndpointSecretValueKey = Environment.GetEnvironmentVariable("RedisClusterConfigurationEndpointSecretValueKey");

                    var serviceProvider = services.BuildServiceProvider();
                    var logger = serviceProvider.GetRequiredService<ILogger<ElastiCacheService>>();
                    var secretsManagerService = serviceProvider.GetRequiredService<IAwsSecretsManagerService>();

                    try
                    {
                        logger.LogInformation($"RedisClusterConfigurationEndpointSecretName - {redisClusterConfigurationEndpointSecretName}\n" +
                            $"RedisClusterConfigurationEndpointSecretValueKey - {redisClusterConfigurationEndpointSecretValueKey}");

                        logger.LogInformation($"Extracting secret for key - {redisClusterConfigurationEndpointSecretValueKey} in {redisClusterConfigurationEndpointSecretName}");
                        var secretKeyValuepairsInJson = secretsManagerService.GetSecretAsync(redisClusterConfigurationEndpointSecretName).ConfigureAwait(false).GetAwaiter().GetResult();
                        var secretValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(secretKeyValuepairsInJson);
                        logger.LogInformation($"Extracted secret values {secretValues}");
                        if (!secretValues!.TryGetValue(redisClusterConfigurationEndpointSecretValueKey!, out var redisConfigurationEndPoint))
                        {
                            throw new Exception("No Redis Cluster configuration endpoint found in secret manager");
                        }
                        logger.LogInformation($"Retrieved Redis endpoint - {redisConfigurationEndPoint}");
                        return redisConfigurationEndPoint;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Error getting Redis Cluster configuration endpoint: {ex.Message}");
                        throw;
                    }
                }

                options.InstanceName = "roundingvNext-cache";
                options.Configuration = redisConfigurationEndPoint;
            });

            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var stackExchangeRedisCacheConfig = provider.GetRequiredService<IOptions<RedisCacheOptions>>();
                var logger = provider.GetRequiredService<Serilog.ILogger>();
                var result = PollyRetry.Execute(() =>
                {
                    return ConnectionMultiplexer.Connect(stackExchangeRedisCacheConfig.Value.Configuration!);
                },
               (result, context, cancellationToken) =>
               {
                   // Log fallback attempt
                   logger.Error($"Fallback attempt to connect with Redis Cluster. RetryCount: {context.PolicyKey}");
                   return default!; // Return a default or fallback value as needed
               },
               (result, context) =>
               {
                    // Log onFallback
                    logger.Error($"OnFallback executed while connecting with Redis Cluster. RetryCount: {context.PolicyKey}");
               },
                logger);
                return result;
            });

            return services;
        }

        public static IServiceCollection AddServiceDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICacheService, ElastiCacheService>();
            return services;
        }

        public static IServiceCollection AddHuronAWSCoreDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSecretsManagerDependencies();
            return services;
        }
    }
}
