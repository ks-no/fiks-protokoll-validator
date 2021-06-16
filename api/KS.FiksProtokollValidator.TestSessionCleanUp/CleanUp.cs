using KS.FiksProtokollValidator.WebAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Microsoft.EntityFrameworkCore;
using KS.FiksProtokollValidator.WebAPI.Models;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace KS.FiksProtokollValidator.TestSessionCleanUp
{
    public class CleanUp : IHostedService
    {
        private readonly FiksIOMessageDBContext _context;
        private readonly int NumberOfDays = -30;
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        public CleanUp(FiksIOMessageDBContext context)
        {
            _context = context;
        }

        public bool DoWork()
        {
            Console.WriteLine("TEST TEST");
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
            return true;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            DoWork();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
