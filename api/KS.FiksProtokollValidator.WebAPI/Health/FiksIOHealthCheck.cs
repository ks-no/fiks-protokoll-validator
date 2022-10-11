using System.Threading;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.FiksIO;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KS.FiksProtokollValidator.WebAPI.Health;

public class FiksIOHealthCheck : IHealthCheck
{
    private readonly FiksResponseMessageService _fiksResponseMessageService;
    
    public FiksIOHealthCheck(FiksResponseMessageService fiksResponseMessageService)
    {
        _fiksResponseMessageService = fiksResponseMessageService;
    }
    
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var isHealthy = _fiksResponseMessageService.isHealthy();
        
        if (isHealthy)
        {
            return Task.FromResult(
                HealthCheckResult.Healthy("Validator API health er ok."));
        }

        return Task.FromResult(
            new HealthCheckResult(
                context.Registration.FailureStatus, "Validator API health er ikke ok."));

    }
}