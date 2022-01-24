using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Models;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.Models;
using KS.FiksProtokollValidator.WebAPI.Payload;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO
{
    public class FiksRequestMessageService : IFiksRequestMessageService
    {
        private readonly Guid _senderId;
        private FiksIOClient _client;
        private AppSettings _appSettings;
        private const int TTLMinutes = 5; 

        public FiksRequestMessageService(AppSettings appAppSettings)
        {
            _appSettings = appAppSettings;
            var config = FiksIOConfigurationBuilder.CreateFiksIOConfiguration(_appSettings);

            _client = new FiksIOClient(config);

            _senderId = config.KontoConfiguration.KontoId;
        }

        public Guid Send(FiksRequest fiksRequest, Guid receiverId)
        {
            var ttl = new TimeSpan(0, TTLMinutes, 0); 
            var messageRequest = new MeldingRequest(_senderId, receiverId, fiksRequest.TestCase.MessageType, ttl);

            var payloads = new List<IPayload>();

            if (fiksRequest.CustomPayloadFile != null)
            {
                PayloadHelper.CreateCustomPayload(fiksRequest, payloads);
            }
            else
            {
                PayloadHelper.CreateStandardPayload(fiksRequest, payloads);
            }

            var attachmentFileNames = fiksRequest.TestCase.PayloadAttachmentFileNames;

            if (!string.IsNullOrEmpty(attachmentFileNames))
            {
                foreach (var payloadFileName in attachmentFileNames.Split(";"))
                {
                    var testCaseDirectory = Path.Combine(TestSeeder.TestsDirectory, fiksRequest.TestCase.Protocol, fiksRequest.TestCase.Operation + fiksRequest.TestCase.Situation);
                    var fileStream = File.OpenRead(Path.Combine(testCaseDirectory, "Attachments", payloadFileName));
                    payloads.Add(new StreamPayload(fileStream, payloadFileName));
                }
            }

            fiksRequest.SentAt = DateTime.Now;
            var result = _client.Send(messageRequest, payloads).Result;

            return result.MeldingId;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
