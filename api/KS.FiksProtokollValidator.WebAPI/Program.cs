using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Network;

namespace KS.FiksProtokollValidator.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logstashDestination = Environment.GetEnvironmentVariable("LOGSTASH_DESTINATION");
            var hostname = Environment.GetEnvironmentVariable("HOSTNAME");
            var kubernetesNode = Environment.GetEnvironmentVariable("KUBERNETES_NODE");
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
            
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Localization", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("app", "digiorden")
                .Enrich.WithProperty("env", environment)
                .Enrich.WithProperty("logsource", hostname)
                .Enrich.WithProperty("node", kubernetesNode)
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] [{RequestId}] [{requestid}] - {Message} {NewLine} {Exception}");

            if (!string.IsNullOrEmpty(logstashDestination))
            {
                loggerConfiguration.WriteTo.TCPSink($"tcp://{logstashDestination}"); 
            }
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureAppConfiguration((config) =>
                {
                    config.AddEnvironmentVariables("fiksProtokollValidator_");
                });
    }
}
