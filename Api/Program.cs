using Caching.AWS.ElastiCache;
using Caching.Api;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Caching.AWS.ElastiCache;

namespace Caching.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = BuildConfiguration();
            //var builder = WebApplication.CreateBuilder(args).UseUrls("http://+:5000", "https://+:5443");
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(5000);
                serverOptions.ListenAnyIP(5443);
            });

            // Add services to the container.
            builder.Services
                .AddSerilogServices(configuration)
                .AddServiceDependencies(configuration)
                .AddRedisDependencies(configuration)
                .AddHuronAWSCoreDependencies(configuration);
            builder.Services.AddControllers();
            builder.Services.AddHealthChecks();
            builder.Services
                .AddEndpointsApiExplorer() // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckl
                .AddSwaggerGen();

            var app = builder.Build();
            try
            {              
                // Configure the HTTP request pipeline.
                if (app.Environment.IsEnvironment("local"))
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
                app.MapHealthChecks("/api/HealthCheck/Diagnose", new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        context.Response.ContentType = "application/json";
                        var result = JsonSerializer.Serialize(
                            new
                            {
                                status = report.Status.ToString(),
                                checks = report.Entries.Select(entry => new
                                {
                                    name = entry.Key,
                                    status = entry.Value.Status.ToString(),
                                    exception = entry.Value.Exception != null ? entry.Value.Exception.Message : "none",
                                    duration = entry.Value.Duration.ToString()
                                })
                            });
                        await context.Response.WriteAsync(result);
                    }
                });
                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();
                app.Run();
            }
            catch(Exception ex) {   
                Console.WriteLine(ex.ToString());
            }
        }

        static IConfiguration BuildConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            if (IsLocalEnvironment())
            {
                configurationBuilder.AddUserSecrets<Program>();
            }

            return configurationBuilder.Build();
        }

        static bool IsLocalEnvironment()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return string.Equals(environment, "local", StringComparison.OrdinalIgnoreCase);
        }
    }
}