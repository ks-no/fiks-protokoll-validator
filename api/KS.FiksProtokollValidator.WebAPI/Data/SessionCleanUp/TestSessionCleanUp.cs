using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KS.FiksProtokollValidator.WebAPI.Data.SessionCleanUp
{
    public class TestSessionCleanUp : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TestSessionCleanUp(IServiceScopeFactory serviceScopeFactory)
        {
            _scopeFactory = serviceScopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            using (var scope = _scopeFactory.CreateScope())
            {
                var worker = scope.ServiceProvider.GetRequiredService<ICleanUpWorker>();
                await worker.DoWork(stoppingToken);
            }
        }
    }
}
