using System;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO;

public interface IFiksProtokolleConnectionService : IAsyncInitialization, IDisposable
{
    public bool IsHealthy();
    public Task Reconnect();
    IFiksIOClient FiksIOClient { get; }
}