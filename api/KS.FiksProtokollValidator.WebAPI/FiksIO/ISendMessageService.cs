using System;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO
{
    public interface ISendMessageService
    {
        Task<Guid> Send(FiksRequest fiksRequest, Guid receiverId, string selectedProtocol);
    }
}
