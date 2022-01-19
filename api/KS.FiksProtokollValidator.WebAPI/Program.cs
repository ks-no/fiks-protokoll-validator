using System;
using System.IO;
using System.Reflection;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Network;

namespace KS.FiksProtokollValidator.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var aspnetcoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var logstashDestination = Environment.GetEnvironmentVariable("LOGSTASH_DESTINATION");
            var hostname = Environment.GetEnvironmentVariable("HOSTNAME");
            var kubernetesNode = Environment.GetEnvironmentVariable("KUBERNETES_NODE");
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
            
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Localization", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("app", "protokoll-validator")
                .Enrich.WithProperty("env", environment)
                .Enrich.WithProperty("logsource", hostname)
                .Enrich.WithProperty("node", kubernetesNode)
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] [{RequestId}] [{requestid}] - {Message} {NewLine} {Exception}");

            if (!string.IsNullOrEmpty(logstashDestination))
            {
                loggerConfiguration.WriteTo.TCPSink($"tcp://{logstashDestination}", new CustomLogstashJsonFormatter()); 
            }
            
            Log.Logger = loggerConfiguration.CreateLogger();
            
            Log.Information("Starting host with env variables:");
            Log.Information("ASPNETCORE_ENVIRONMENT: {AspnetcoreEnvironment}", aspnetcoreEnvironment);
            Log.Information("HOSTNAME: {Hostname}", hostname);
            Log.Information("KUBERNETES_NODE: {KubernetesNode}", kubernetesNode);
            Log.Information("ENVIRONMENT: {Environment}",environment);
            Log.Information("LOGSTASH_DESTINATION: {LogstashDestination}", logstashDestination);
            Log.Information("Path.PathSeparator: {PathSeparator}", Path.PathSeparator);

            var host = CreateHostBuilder(args).Build();
            
            MigrateAndSeedDatabase(host);
            
            host.Run();
        }
        
        private static void MigrateAndSeedDatabase(IHost host)
        {
            Log.Information("Running migrations and seeding data");
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<FiksIOMessageDBContext>();
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureAppConfiguration((config) =>
                {
                    config.AddEnvironmentVariables("fiksProtokollValidator_");
                })
                .UseSerilog();
    }
}
