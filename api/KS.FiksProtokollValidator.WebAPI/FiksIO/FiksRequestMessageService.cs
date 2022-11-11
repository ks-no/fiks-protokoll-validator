using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;
using KS.FiksProtokollValidator.WebAPI.Models;
using KS.FiksProtokollValidator.WebAPI.Payload;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO
{
    
    /* This is the producer that sends messages to Fiks-Protokoller/Fiks-IO
     * The FiksIOClient is only used for sending and we are not using the Fiks-IO connection for receiving messages
     * That means we are not interested in any health check or keepAlive for this Fiks-IO connection
     */
    public class FiksRequestMessageService : IFiksRequestMessageService
    {
        private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly Guid _senderId;
        private AppSettings _appSettings;
        private const int TTLMinutes = 5;
        private readonly FiksIOConfiguration _config;
        private FiksIOClient Client { get; set; }


        public FiksRequestMessageService(AppSettings appAppSettings)
        {
            _appSettings = appAppSettings;
            _config = FiksIOConfigurationBuilder.CreateFiksIOConfiguration(_appSettings);
            _senderId = _config.KontoConfiguration.KontoId;
            Initialization = InitializeAsync();
        }

        private Task Initialization { get; set; }

        private async Task InitializeAsync()
        {
            Client = await FiksIOClient.CreateAsync(_config);
        }

        public async Task<Guid> Send(FiksRequest fiksRequest, Guid receiverId)
        {
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
            var result = await Client.Send(messageRequest, payloads).ConfigureAwait(false);

            return result.MeldingId;
        }

        public void Dispose()
        {
            Logger.Information("FiksRequestMessageService: Dispose");
            Client?.Dispose();
        }
    }
}
