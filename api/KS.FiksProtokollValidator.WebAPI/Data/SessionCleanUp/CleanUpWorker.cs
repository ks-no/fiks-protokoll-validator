using KS.FiksProtokollValidator.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.Data.SessionCleanUp
{
    public class CleanUpWorker : ICleanUpWorker
    {
        private readonly IUnitOfWorkManager _manager;
        private readonly int NumberOfDays = -30;
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        public CleanUpWorker(IUnitOfWorkManager manager)
        {
            _manager = manager;
        }
        public async Task DoWork(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var uow = _manager.GetUnitOfWork()) 
                {
                    Log.Information($"Starting to delete records older than {NumberOfDays * -1} days in database.");
                    var testSessions = uow.Context.TestSessions.Include(e => e.FiksRequests).ThenInclude(e => e.FiksResponses);


                    foreach (TestSession testSession in testSessions)
                    {

                        if (testSession.CreatedAt < DateTime.Now.AddDays(NumberOfDays))
                        {
                            foreach (FiksRequest fiksRequest in testSession.FiksRequests)
                            {
                                foreach (FiksResponse fiksResponse in fiksRequest.FiksResponses)
                                {
                                    Log.Information($"Deleting FiksResponse: {fiksResponse.Id}");
                                    uow.Context.FiksResponse.Remove(fiksResponse);
                                }
                                Log.Information($"Deleting FiksRequest: {fiksRequest.MessageGuid}");
                                uow.Context.FiksRequest.Remove(fiksRequest);
                            }
                            Log.Information($"Deleting TestSession: {testSession.Id}");
                            uow.Context.TestSessions.Remove(testSession);
                        }
                    }
                    uow.Context.SaveChanges();
                    Log.Information("Deleting of records completed");
                    await Task.Delay(3600000 * 24);
                };
            }
        }
    }
}
