using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO.Connection
{
    public class MockSendMessageService : ISendMessageService
    {
        public Task<Guid> Send(FiksRequest fiksRequest, Guid receiverId, string selectedProtocol)
        {
            var messageId = Guid.NewGuid();

            fiksRequest.SentAt = DateTime.Now;
            fiksRequest.FiksResponses ??= new List<FiksResponse>();
            fiksRequest.FiksResponses.Add(new FiksResponse
            {
                ReceivedAt = DateTime.Now,
                Type = fiksRequest.TestCase.MessageType + ".resultat",
                IsAsiceVerified = true,
                PayloadErrors = string.Empty
            });

            return Task.FromResult(messageId);
        }
    }
}
