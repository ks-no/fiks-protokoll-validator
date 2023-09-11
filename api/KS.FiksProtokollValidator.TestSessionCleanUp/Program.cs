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
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            const int numberOfDays = -30;

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Localization", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("app", "protokoll-validator")
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] [{RequestId}] [{requestid}] - {Message} {NewLine} {Exception}");

            Log.Logger = loggerConfiguration.CreateLogger();

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var options = new DbContextOptionsBuilder<FiksIOMessageDBContext>()
                .UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .Options;
            var context = new FiksIOMessageDBContext(options);

            Log.Information($"Starting to delete records older than {numberOfDays * -1} days in database.");
            var testSessions = context.TestSessions.Include(e => e.FiksRequests).ThenInclude(e => e.FiksResponses);

            foreach (var testSession in testSessions)
            {

                if (testSession.CreatedAt < DateTime.Now.AddDays(numberOfDays))
                {
                    foreach (var fiksRequest in testSession.FiksRequests)
                    {
                        foreach (var fiksResponse in fiksRequest.FiksResponses)
                        {
                            Log.Information($"Deleting FiksResponse: {fiksResponse.Id}");
                            context.FiksResponse.Remove(fiksResponse);
                        }
                        Log.Information($"Deleting FiksRequest: {fiksRequest.MessageGuid}");
                        context.FiksRequest.Remove(fiksRequest);
                    }
                    Log.Information($"Deleting TestSession: {testSession.Id}");
                    context.TestSessions.Remove(testSession);
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
