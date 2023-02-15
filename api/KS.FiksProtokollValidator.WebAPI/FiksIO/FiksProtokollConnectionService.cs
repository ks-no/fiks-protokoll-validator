using System.Reflection;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO;

public class FiksProtokollConnectionService : IFiksProtokolleConnectionService
{
    private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
    public IFiksIOClient FiksIOClient { get; private set; }
    private readonly FiksProtokollConsumerServiceSettings _consumerServiceSettings;

    public FiksProtokollConnectionService(FiksProtokollConsumerServiceSettings fiksProtokollKontoConfig)
    {
        _consumerServiceSettings = fiksProtokollKontoConfig;
        Initialization = InitializeAsync();
    }

    public Task Initialization { get; private set; }

    private async Task InitializeAsync()
    {
        FiksIOClient = await Fiks.IO.Client.FiksIOClient.CreateAsync(FiksIOConfigurationBuilder.CreateFiksIOConfiguration(_consumerServiceSettings));
    }

    public bool IsHealthy()
    {
        if (FiksIOClient != null)
        {
            var status = FiksIOClient.IsOpen();
            Logger.Debug($"FiksIOClientConsumerService: FiksIOCClient.IsOpen() returns {status}");
            return status;
        }
        Logger.Error("FiksIOClientConsumerService: FiksIOClient is null. Returning not healthy.");
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