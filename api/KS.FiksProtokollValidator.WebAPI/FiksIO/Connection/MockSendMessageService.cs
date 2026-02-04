using System;
using System.Collections.Generic;
using System.Text;
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
                PayloadErrors = string.Empty,
                FiksPayloads = new List<FiksPayload>
                {
                    new FiksPayload
                    {
                        Filename = "arkivmeldingKvittering.xml",
                        Payload = Encoding.UTF8.GetBytes(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<arkivmeldingKvittering xmlns=""http://www.arkivverket.no/standarder/noark5/arkivmelding/v2"">
  <mappeKvittering xsi:type=""saksmappeKvittering"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <systemID>12345</systemID>
    <saksaar>2026</saksaar>
    <sakssekvensnummer>1</sakssekvensnummer>
  </mappeKvittering>
</arkivmeldingKvittering>")
                    }
                }
            });

            return Task.FromResult(messageId);
        }
    }
}
