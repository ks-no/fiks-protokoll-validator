using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using KS.FiksProtokollValidator.WebAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace KS.FiksProtokollValidator.TestSessionCleanUp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();


            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Localization", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("app", "protokoll-validator")
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] [{RequestId}] [{requestid}] - {Message} {NewLine} {Exception}");


            Log.Logger = loggerConfiguration.CreateLogger();

            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

                services.AddDbContext<FiksIOMessageDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
                services.AddHostedService<CleanUp>();
            }).UseSerilog();
    }
}
