using System.Collections.Generic;
using System.IO;
using System.Reflection;
using KS.Fiks.IO.Crypto.Models;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models;

namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Utilities.Payload
{
    public class PayloadHelper
    {
        public static void CreateStandardPayload(FiksRequest fiksRequest, List<IPayload> payloads)
        {
            var payLoadFileName = fiksRequest.TestCase.PayloadFileName;

            if (string.IsNullOrEmpty(payLoadFileName)) return;
            
            IPayload payload = new StringPayload(GetStandardPayloadAsText(fiksRequest), payLoadFileName);
            payloads.Add(payload);
        }

        public static string GetStandardPayloadAsText(FiksRequest fiksRequest)
        {
            var basepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var payLoadFilePath = fiksRequest.TestCase.SamplePath;
            var payloadFileName = fiksRequest.TestCase.PayloadFileName;
            var completePath = Path.GetFullPath(basepath + "/" + payLoadFilePath + "/" + payloadFileName);
            return File.ReadAllText(completePath);
        }

        public static void CreateCustomPayload(FiksRequest fiksRequest, List<IPayload> payloads)
        {
            IPayload payload = new StreamPayload(new MemoryStream(fiksRequest.CustomPayloadFile.Payload),
                fiksRequest.CustomPayloadFile.Filename);
            payloads.Add(payload);
        }
    }
}