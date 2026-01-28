using System;
using System.Diagnostics.Tracing;
using System.IO;
using KS.Fiks.IO.Client.Amqp.RabbitMQ;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.Health;
using KS.FiksProtokollValidator.WebAPI.Utilities.Logging;
using Microsoft.AspNetCore.Builder;
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
        private static string _hostname;
        private static string _environment;
        private static string _kubernetesNode;
        private static string _logstashDestination;

        public static void Main(string[] args)
        {
            var aspnetcoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            _logstashDestination = Environment.GetEnvironmentVariable("LOGSTASH_DESTINATION");
            _hostname = Environment.GetEnvironmentVariable("HOSTNAME");
            _kubernetesNode = Environment.GetEnvironmentVariable("KUBERNETES_NODE");
            _environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
            
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Localization", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("app", "protokoll-validator")
                .Enrich.WithProperty("env", _environment)
                .Enrich.WithProperty("logsource", _hostname)
                .Enrich.WithProperty("node", _kubernetesNode)
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] [{SourceContext}] - {Message} {NewLine} {Exception}");

            if (!string.IsNullOrEmpty(_logstashDestination))
            {
                loggerConfiguration.WriteTo.TCPSink($"tcp://{_logstashDestination}", new CustomLogstashJsonFormatter()); 
            }
            
            Log.Logger = loggerConfiguration.CreateLogger();
            var loggerFactory = InitSerilogConfiguration();

            Log.Information("Starting host with env variables:");
            Log.Information("ASPNETCORE_ENVIRONMENT: {AspnetcoreEnvironment}", aspnetcoreEnvironment);
            Log.Information("HOSTNAME: {Hostname}", _hostname);
            Log.Information("KUBERNETES_NODE: {KubernetesNode}", _kubernetesNode);
            Log.Information("ENVIRONMENT: {Environment}",_environment);
            Log.Information("LOGSTASH_DESTINATION: {LogstashDestination}", _logstashDestination);
            Log.Information("Path.PathSeparator: {PathSeparator}", Path.PathSeparator);

            var appBuilder = CreateHostBuilder(args);
            var standaloneMode = IsStandaloneMode(appBuilder.Configuration);

            var startup = new Startup(appBuilder.Configuration);
            startup.ConfigureServices(appBuilder.Services, loggerFactory);

            var app = appBuilder.Build();

            app.MapHealthChecks("/api/healthz");

            MigrateAndSeedDatabase(app, standaloneMode);

            startup.Configure(app, appBuilder.Environment);

            if (!standaloneMode)
            {
                var rabbitMqLogger = new RabbitMQEventLogger(loggerFactory, EventLevel.Informational);
            }

            Log.Information("WebApplication api started running with urls: ");
            foreach (var appUrl in app.Urls)
            {
                Log.Information(appUrl);
            }
            
            app.Run();
        }
        
        private static void MigrateAndSeedDatabase(IHost host, bool standaloneMode)
        {
            Log.Information("Running migrations and seeding data");
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<FiksIOMessageDBContext>();
                    if (standaloneMode)
                    {
                        context.Database.EnsureCreated();
                    }
                    else
                    {
                        context.Database.Migrate();
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
        }

        private static bool IsStandaloneMode(IConfiguration configuration)
        {
            return configuration.GetValue<bool>("AppSettings:StandaloneMode");
        }

        private static WebApplicationBuilder CreateHostBuilder(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddEnvironmentVariables("fiksProtokollValidator_");

            if (!IsStandaloneMode(builder.Configuration))
            {
                builder.Services.AddHealthChecks().AddCheck<FiksIOHealthCheck>("FiksIO");
            }
            else
            {
                builder.Services.AddHealthChecks();
            }

            builder.Host.UseSerilog();
            return builder;
        }
        
        private static ILoggerFactory InitSerilogConfiguration()
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Localization", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("app", "protokoll-validator")
                .Enrich.WithProperty("env", _environment)
                .Enrich.WithProperty("logsource", _hostname)
                .Enrich.WithProperty("node", _kubernetesNode)
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] [{RequestId}] [{requestid}] - {Message} {NewLine} {Exception}");
            
            if (!string.IsNullOrEmpty(_logstashDestination))
            {
                loggerConfiguration.WriteTo.TCPSink($"tcp://{_logstashDestination}", new CustomLogstashJsonFormatter()); 
            }
            
            var logger = loggerConfiguration.CreateLogger();
            Log.Logger = logger;

            return LoggerFactory.Create(logging => logging.AddSerilog(logger));
        }
    }
}
