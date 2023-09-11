using System.IO;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Helpers.Payload;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models;

namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation
{
    public abstract class AbstractXmlValidator : AbstractValidator
    {
        public static string GetStandardRequestXml(FiksRequest fiksRequest)
        {
            return PayloadHelper.GetStandardPayloadAsText(fiksRequest);
        }

        public static string GetCustomRequestXml(FiksRequest fiksRequest)
        {
            var stream = new StreamReader(new MemoryStream(fiksRequest.CustomPayloadFile.Payload));
            return stream.ReadToEnd();
        }
    }
}