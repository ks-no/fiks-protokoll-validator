using System.Reflection;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;
using KS.FiksProtokollValidator.WebAPI.FiksIO.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Serilog.ILogger;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO.Connection;

public class FiksIOConnectionService : IFiksIOConnectionService
{
    private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
    private static ILoggerFactory _loggerFactory;
    public IFiksIOClient FiksIOClient { get; private set; }
    private readonly FiksProtokollKontoConfig _kontoConfig;
    
    public FiksIOConnectionService(FiksProtokollKontoConfig fiksProtokollKontoConfig, ILoggerFactory loggerFactory)
    {
        _kontoConfig = fiksProtokollKontoConfig;
        _loggerFactory = loggerFactory;
        Initialization = InitializeAsync(loggerFactory);
    }

    public Task Initialization { get; private set; }

    private async Task InitializeAsync(ILoggerFactory loggerFactory)
    {
        FiksIOClient = await Fiks.IO.Client.FiksIOClient.CreateAsync(FiksIOConfigurationBuilder.CreateFiksIOConfiguration(_kontoConfig));
    }

    public async Task<bool> IsHealthy()
    {
        if (FiksIOClient != null)
        {
            var status = await FiksIOClient.IsOpenAsync();
            Logger.Debug($"FiksIOCClient.IsOpenAsync() returns {status}");
            return status;
        }
        Logger.Error("FiksIOClient is null. Returning not healthy.");
        return false;
    }

    public async Task Reconnect()
    {
       await FiksIOClient.DisposeAsync();
        Initialization = InitializeAsync(_loggerFactory);
        await Initialization;
    }
    

    public async ValueTask DisposeAsync()
    {
        await FiksIOClient.DisposeAsync();
        Initialization?.Dispose();
    }
}