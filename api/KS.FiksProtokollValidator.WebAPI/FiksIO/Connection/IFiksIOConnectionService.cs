using System;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO.Connection;

public interface IFiksIOConnectionService : IAsyncInitialization, IAsyncDisposable
{
    public Task<bool> IsHealthy();
    public Task Reconnect();
    IFiksIOClient FiksIOClient { get; }
}