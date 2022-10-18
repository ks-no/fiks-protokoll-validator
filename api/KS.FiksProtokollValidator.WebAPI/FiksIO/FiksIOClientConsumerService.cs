using System.Reflection;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO;

public class FiksIOClientConsumerService : IFiksIOClientConsumerService
{
    private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
    public IFiksIOClient FiksIOConsumerClient { get; private set; }
    private AppSettings _appSettings;

    public FiksIOClientConsumerService(AppSettings appAppSettings)
    {
        _appSettings = appAppSettings;
        Initialization = InitializeAsync();
    }

    public Task Initialization { get; private set; }

    private async Task InitializeAsync()
    {
        FiksIOConsumerClient = await FiksIOClient.CreateAsync(FiksIOConfigurationBuilder.CreateFiksIOConfiguration(_appSettings));
    }

    public bool IsHealthy()
    {
        if (FiksIOConsumerClient != null)
        {
            var status = FiksIOConsumerClient.IsOpen();
            Logger.Debug($"FiksIOClientConsumerService: FiksIOCClient.IsOpen() returns {status}");
            return status;
        }
        Logger.Error("FiksIOClientConsumerService: FiksIOClient is null. Returning not healthy.");
        return false;
    }

    public async Task Reconnect()
    {
        FiksIOConsumerClient.Dispose();
        Initialization = InitializeAsync();
    }
}