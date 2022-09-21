using System.Threading.Tasks;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO;

public interface IAsyncInitialization
{
    /// <summary>
    /// The result of the asynchronous initialization of this instance.
    /// </summary>
    Task Initialization { get; }
}