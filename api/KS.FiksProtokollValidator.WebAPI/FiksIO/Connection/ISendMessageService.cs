using System;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO.Connection
{
    public interface ISendMessageService
    {
        Task<Guid> Send(FiksRequest fiksRequest, Guid receiverId, string selectedProtocol);
    }
}
