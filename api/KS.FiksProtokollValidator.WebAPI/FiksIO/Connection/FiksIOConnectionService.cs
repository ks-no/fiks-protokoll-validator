using System.Reflection;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;
using KS.FiksProtokollValidator.WebAPI.FiksIO.Configuration;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO.Connection;

public class FiksIOConnectionService : IFiksIOConnectionService
{
    private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
    public IFiksIOClient FiksIOClient { get; private set; }
    private readonly FiksProtokollKontoConfig _kontoConfig;

    public FiksIOConnectionService(FiksProtokollKontoConfig fiksProtokollKontoConfig)
    {
        _kontoConfig = fiksProtokollKontoConfig;
        Initialization = InitializeAsync();
    }

    public Task Initialization { get; private set; }

    private async Task InitializeAsync()
    {
        FiksIOClient = await Fiks.IO.Client.FiksIOClient.CreateAsync(FiksIOConfigurationBuilder.CreateFiksIOConfiguration(_kontoConfig));
    }

    public bool IsHealthy()
    {
        if (FiksIOClient != null)
        {
            var status = FiksIOClient.IsOpen();
            Logger.Debug($"FiksIOCClient.IsOpen() returns {status}");
            return status;
        }
        Logger.Error("FiksIOClient is null. Returning not healthy.");
        return false;
    }

    public async Task Reconnect()
    {
        FiksIOClient.Dispose();
        Initialization = InitializeAsync();
        await Initialization;
    }

    public void Dispose()
    {
        FiksIOClient?.Dispose();
        Initialization?.Dispose();
    }
}