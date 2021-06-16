using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using KS.FiksProtokollValidator.WebAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using KS.FiksProtokollValidator.WebAPI.Models;

namespace KS.FiksProtokollValidator.TestSessionCleanUp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int NumberOfDays = -30;

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Localization", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("app", "protokoll-validator")
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] [{RequestId}] [{requestid}] - {Message} {NewLine} {Exception}");

            Log.Logger = loggerConfiguration.CreateLogger();

            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var options = new DbContextOptionsBuilder<FiksIOMessageDBContext>()
                .UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                .Options;
            FiksIOMessageDBContext _context = new FiksIOMessageDBContext(options);

            Log.Information($"Starting to delete records older than {NumberOfDays * -1} days in database.");
            var testSessions = _context.TestSessions.Include(e => e.FiksRequests).ThenInclude(e => e.FiksResponses);

            foreach (TestSession testSession in testSessions)
            {

                if (testSession.CreatedAt < DateTime.Now.AddDays(NumberOfDays))
                {
                    foreach (FiksRequest fiksRequest in testSession.FiksRequests)
                    {
                        foreach (FiksResponse fiksResponse in fiksRequest.FiksResponses)
                        {
                            Log.Information($"Deleting FiksResponse: {fiksResponse.Id}");
                            _context.FiksResponse.Remove(fiksResponse);
                        }
                        Log.Information($"Deleting FiksRequest: {fiksRequest.MessageGuid}");
                        _context.FiksRequest.Remove(fiksRequest);
                    }
                    Log.Information($"Deleting TestSession: {testSession.Id}");
                    _context.TestSessions.Remove(testSession);
                }
            }
            _context.SaveChanges();
        }
    }
}
