using System.Threading;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.FiksIO;
using KS.FiksProtokollValidator.WebAPI.FiksIO.Connection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KS.FiksProtokollValidator.WebAPI.Health;

public class FiksIOHealthCheck : IHealthCheck
{
    private readonly IFiksIOConnectionService _fiksIoClientService;
    
    public FiksIOHealthCheck(IFiksIOConnectionService fiksIoClientService)
    {
        _fiksIoClientService = fiksIoClientService;
    }
    
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        if (!_fiksIoClientService.IsHealthy())
        {
            return Task.FromResult(
                new HealthCheckResult(
                    context.Registration.FailureStatus, "Validator API health er ikke ok. FiksIOClient for consumer er ikke healthy."));
        }

        return Task.FromResult(
            HealthCheckResult.Healthy("Validator API health er ok."));
    }
}