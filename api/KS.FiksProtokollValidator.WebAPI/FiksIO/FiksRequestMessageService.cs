using System;
using System.Collections.Generic;
using System.IO;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Models;
using KS.FiksProtokollValidator.WebAPI.Models;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO
{
    public class FiksRequestMessageService : IFiksRequestMessageService
    {
        private readonly Guid _senderId;
        private FiksIOClient _client;
        private AppSettings _appSettings;
        private const int TtlMinutes = 5; 

        public FiksRequestMessageService(AppSettings appAppSettings)
        {
            _appSettings = appAppSettings;
            var config = FiksIOConfigurationBuilder.CreateFiksIOConfiguration(_appSettings);

            _client = new FiksIOClient(config);

            _senderId = config.KontoConfiguration.KontoId;
        }

        public Guid Send(FiksRequest fiksRequest, Guid receiverId)
        {
            var ttl = new TimeSpan(0, TtlMinutes, 0); 
            var messageRequest = new MeldingRequest(_senderId, receiverId, fiksRequest.TestCase.MessageType, ttl);

            var payloads = new List<IPayload>();

            var payLoadFileName = fiksRequest.TestCase.PayloadFileName;
            var testsDirectory = "TestCases/";
            var testCaseDirectory = Path.Combine(testsDirectory, fiksRequest.TestCase.Protocol, fiksRequest.TestCase.Operation + fiksRequest.TestCase.Situation);

            if (!string.IsNullOrEmpty(payLoadFileName))
            {
                var payLoadFilePath = Path.Combine(testCaseDirectory, payLoadFileName);

                IPayload payload = new StringPayload(File.ReadAllText(payLoadFilePath), payLoadFileName);

                payloads.Add(payload);
            }

            var attachmentFileNames = fiksRequest.TestCase.PayloadAttachmentFileNames;

            if (!string.IsNullOrEmpty(attachmentFileNames))
            {
                foreach (var payloadFileName in attachmentFileNames.Split(";"))
                {
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
