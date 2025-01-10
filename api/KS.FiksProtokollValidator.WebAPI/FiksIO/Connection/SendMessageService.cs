using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Crypto.Models;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Utilities.Payload;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.FiksIO.Connection
{
    
    /* This is the producer that sends messages to Fiks-Protokoller/Fiks-IO
     */
    //TODO We could try using the Fiks-IO-Send client instead of the shared Fiks-IO-client as an example of how it could be used
    public class SendMessageService : ISendMessageService
    {
        private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
        private const int TTLMinutes = 60;
        private readonly FiksIOConfiguration _config;
        private FiksIOConnectionManager _fiksIOConnectionManager;

        public SendMessageService(FiksIOConnectionManager fiksIOConnectionManager)
        {
            _fiksIOConnectionManager = fiksIOConnectionManager;
        }

        public async Task<Guid> Send(FiksRequest fiksRequest, Guid receiverId, string selectedProtocol)
        {
            var foundProtocol = _fiksIOConnectionManager.TjenerConnectionServices.TryGetValue(selectedProtocol, out var connectionService);

            if (!foundProtocol)
            {
                Log.Error($"Did not find any connection service for the protocol {selectedProtocol}");
                throw new Exception($"Did not find any connection service for the protocol {selectedProtocol}");
            }

            await connectionService.Initialization;
            
            var testName = fiksRequest.TestCase.Operation + fiksRequest.TestCase.Situation;
            var headers = new Dictionary<string, string>() { { "protokollValidatorTestName", testName } };
            var ttl = new TimeSpan(0, TTLMinutes, 0); 
            var messageRequest = new MeldingRequest(connectionService.FiksIOClient.KontoId, receiverId, fiksRequest.TestCase.MessageType, ttl, headers);

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
            var result = await connectionService.FiksIOClient.Send(messageRequest, payloads).ConfigureAwait(false);

            return result.MeldingId;
        }
    }
}
