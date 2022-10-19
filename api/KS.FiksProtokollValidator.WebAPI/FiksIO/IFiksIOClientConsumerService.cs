using System.Threading.Tasks;
using KS.Fiks.IO.Client;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO;

public interface IFiksIOClientConsumerService : IAsyncInitialization
{
    public bool IsHealthy();
    public Task Reconnect();
    IFiksIOClient FiksIOConsumerClient { get; }
}