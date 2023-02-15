using System;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.Models;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO
{
    public interface IFiksRequestMessageService
    {
        Task<Guid> Send(FiksRequest fiksRequest, Guid receiverId, string selectedProtocol);
    }
}
