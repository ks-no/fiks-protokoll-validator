using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.Models;
using KS.FiksProtokollValidator.WebAPI.Payload;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO
{
    public class FiksRequestMessageService : IFiksRequestMessageService, IAsyncInitialization
    {
        private readonly Guid _senderId;
        private FiksIOClient _client;
        private AppSettings _appSettings;
        private const int TTLMinutes = 5;
        private readonly FiksIOConfiguration _config;

        public FiksRequestMessageService(AppSettings appAppSettings)
        {
            _appSettings = appAppSettings;
            _config = FiksIOConfigurationBuilder.CreateFiksIOConfiguration(_appSettings);
            _senderId = _config.KontoConfiguration.KontoId;
            Initialization = InitializeAsync();
        }

        public Task Initialization { get; private set; }
        
        private async Task InitializeAsync()
        {
            _client = await FiksIOClient.CreateAsync(_config);
        }

        public async Task<Guid> Send(FiksRequest fiksRequest, Guid receiverId)
        {
            // await FiksIOClient initialization
            await Initialization;
            
            var testName = fiksRequest.TestCase.Operation + fiksRequest.TestCase.Situation;
            var headere = new Dictionary<string, string>() { { "protokollValidatorTestName", testName } };
            var ttl = new TimeSpan(0, TTLMinutes, 0); 
            var messageRequest = new MeldingRequest(_senderId, receiverId, fiksRequest.TestCase.MessageType, ttl, headere);

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
                    var testCaseDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fiksRequest.TestCase.SamplePath);
                    var fileStream = File.OpenRead(Path.Combine(testCaseDirectory, "Attachments", payloadFileName));
                    payloads.Add(new StreamPayload(fileStream, payloadFileName));
                }
            }

            fiksRequest.SentAt = DateTime.Now;
            var result = await _client.Send(messageRequest, payloads);

            return result.MeldingId;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
