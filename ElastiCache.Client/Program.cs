using Amazon.CloudWatchLogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AwsCloudWatch;
using StackExchange.Redis;
using Huron.AWS.SecretsManager.Core;
using Newtonsoft.Json;
using ElastiCache.Client.Caching;
using System.Configuration;
using Microsoft.Extensions.Logging;

namespace ElastiCache.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Build the configuration
                var configuration = BuildConfiguration();

                // Build the service provider
                var serviceProvider = BuildServiceProvider(configuration);

                //var secretsManagerService = serviceProvider.GetRequiredService<IAwsSecretsManagerService>();
                //var searchKey = "roundingvnext-cache-dev";
                //var secretKeyValuepairsInJson = await secretsManagerService.GetSecretAsync(searchKey);
                //var secretValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(secretKeyValuepairsInJson);

                //// Retrieve the specific key-value pair
                //if (secretValues?.ContainsKey("RedisClusterConfigurationEndpoint") == false) throw new Exception("No endpoint found");

                //var endpoint = secretValues?["RedisClusterConfigurationEndpoint"];
                //var redisConfiguration = new ConfigurationOptions
                //{
                //    //EndPoints = {}
                //    EndPoints = {
                //        endpoint!
                //        //"172.17.0.2:6379"
                //        //"huroelasticachecluster-0001-002.huroelasticachecluster.uwfppj.use2.cache.amazonaws.com:6379", 
                //        //"huroelasticachecluster-0001-001.huroelasticachecluster.uwfppj.use2.cache.amazonaws.com:6379" 
                //    }
                //};

                //var cache = ConnectionMultiplexer.Connect(redisConfiguration);
                //// Get Redis cache instance
                //var db = cache.GetDatabase();

                //// Basic Redis commands    
                //db.StringSet("testKey", "testValue");
                //var value = db.StringGet("testKey");
                using (var scope = serviceProvider.CreateScope())
                {
                    // Resolve services within the scope
                    var scopedServiceProvider = scope.ServiceProvider;
                    var cachingService = scopedServiceProvider.GetService<ICachingService>();
                    //var response=await cachingService!.HealthCheckAsync();
                    //var getDataResponse = await cachingService!.GetAsync<string>("key1");
                    //var sampleSetHshResponse= await cachingService!.HashSetAsync<string>("Person:1", "name", "Dipsy");
                    var address= new Address() { City = "B'lore", Country = "IND", Line1 = "SLS Square", Line2 = "EPIP PH-2", PinCode = 560066, State = "K'TAKA", Street = "Whitefield" };
                    var employee = new Employee() { Name = "Subasish", Age = 39, Dept = "HC", Id = 121 
                        //,Address=address
                    };
                    var setHashDataResponse = await cachingService!.HashSetAsync<Employee>($"Emp:{employee.Id}","data", employee);
                    setHashDataResponse = await cachingService!.HashSetAsync<Address>($"Emp:{employee.Id}", "address", address);
                    var getEmployeeResponse = await cachingService!.HashGetAsync<Employee>($"Emp:{employee.Id}", "data");
                    var getAddressResponse = await cachingService!.HashGetAsync<Address>($"Emp:{employee.Id}", "address");
                    var getEmployeeDetailResponse = await cachingService!.HashGetAllAsync($"Emp:{employee.Id}");
                    var deleteEmployeeResponse = await cachingService!.HashDeleteAsync($"Emp:{employee.Id}", $"data");
                    var deleteAddressResponse = await cachingService!.HashDeleteAsync($"Emp:{employee.Id}", $"address");
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message); ;
            }
        }

        // Function to insert data into ElastiCache
        public static void SetData(string data)
        {
            // Connect to ElastiCache Redis cluster
            var cache = ConnectionMultiplexer.Connect(
                "clustercfg.huroelasticachecluster.uwfppj.use2.cache.amazonaws.com:6379");

            // Get Redis cache instance
            var db = cache.GetDatabase();

            // Basic Redis commands    
            db.StringSet("key1", data);
        }

        // Function to insert data into ElastiCache from local machine using access key id and secret access key
        public static void SetData(string key, string data)
        {

            // Connect to ElastiCache Redis cluster from local machine using access key id and secret access key
            //var cache = ConnectionMultiplexer.Connect()
            //var cache = ConnectionMultiplexer.Connect(
            //    "clustercfg.huroelasticachecluster.uwfppj.use2.cache.amazonaws.com:6379");

            //// Get Redis cache instance
            //var db = cache.GetDatabase();

            //// Basic Redis commands    
            //db.StringSet(key, data);

        }

        static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<Program>()
                .Build();
        }
        static IServiceProvider BuildServiceProvider(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            // Add the logging services
            services.AddLogging(builder =>
            {
                builder.AddSerilog();
            });
            // Add any other services you may need
            ConfigureLogger(configuration);

            void ConfigureLogger(IConfiguration configuration)
            {
                string? logGroupName = configuration["Serilog:WriteTo:1:Args:logGroupName"];
                string? region = configuration["Serilog:WriteTo:1:Args:region"];
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
            services.AddSecretsManagerDependencies();

            services.Configure<CacheHttpSettings>(configuration.GetSection("CacheHttpSettings"));
            services.AddScoped<ICacheHttpClient, CacheHttpClient>();
            services.AddScoped<ICachingService, CachingService>();
            // Register the CachingService with the cancellation token
            //services.AddScoped<ICachingService>(provider =>
            //{
            //    // Create a cancellation token source
            //    var cancellationTokenSource = new CancellationTokenSource();

            //    // Example: Set up health check or timeout cancellation logic
            //    // cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30)); // Cancel after 30 seconds

            //    // Pass the cancellation token to the CachingService constructor
            //    return new CachingService(
            //        provider.GetRequiredService<ICacheHttpClient>(),
            //        provider.GetRequiredService<ILogger<CachingService>>(),
            //        cancellationTokenSource.Token);
            //});
            services.AddTypedHttpClients<ICacheHttpClient, CacheHttpClient, CacheHttpSettings>();

            return services.BuildServiceProvider();
        }
        static void ConfigureLogger(IConfiguration configuration)
        {
            string? logGroupName = configuration["Serilog:WriteTo:1:Args:logGroupName"];
            string? region = configuration["Serilog:WriteTo:1:Args:region"];
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
        }

    }
}