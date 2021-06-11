using System.Threading;
using System.Threading.Tasks;

namespace KS.FiksProtokollValidator.WebAPI.Data.SessionCleanUp
{
    public interface ICleanUpWorker
    {
        Task DoWork(CancellationToken cancellationToken);
    }
}