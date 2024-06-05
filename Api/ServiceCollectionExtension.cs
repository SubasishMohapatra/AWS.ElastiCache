using Amazon.CloudWatchLogs;
using Caching.Core;
using Caching.Core;
using Microsoft.Extensions.Logging.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AwsCloudWatch;

namespace Caching.Api
{
    public static class ServiceCollectionExtension
    {     
        //public static IServiceCollection AddLoggingDependencies(this IServiceCollection services, IConfiguration configuration)
        //{
        //    services.AddLogging(builder =>
        //    {
        //        builder.AddConfiguration(configuration.GetSection("Logging")); // Add logging configuration from the appsettings.json or other configuration sources
        //        builder.AddAWSProvider(); // Add AWS CloudWatch logger provider
        //    });
        //    return services;
        //}
        public static IServiceCollection AddSerilogServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add the logging services
            services.AddLogging(builder =>
            {
                builder.AddSerilog();
            });
            // Add any other services you may need
            ConfigureLogger(configuration);

            void ConfigureLogger(IConfiguration configuration)
            {
                string? logGroupName = configuration[Constants.SerilogWriteToLogGroupName];
                string? region = configuration[Constants.AWSConfigRegion];
                var logger = new LoggerConfiguration()
               .MinimumLevel.Information()
               .WriteTo.Console()
               .WriteTo.AmazonCloudWatch(new CloudWatchSinkOptions
               {
                   LogGroupName = logGroupName,
                   LogStreamNameProvider = new DefaultLogStreamProvider(),
                   TextFormatter = new Serilog.Formatting.Json.JsonFormatter(),
                   MinimumLogEventLevel = LogEventLevel.Information
               }, new AmazonCloudWatchLogsClient(new AmazonCloudWatchLogsConfig { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region) }))
               .CreateLogger();
                services.AddSingleton<Serilog.ILogger>(logger);
            }
            // Build the service provider
            services.AddSingleton<IConfiguration>(configuration);
            return services;
        }
    }
}
