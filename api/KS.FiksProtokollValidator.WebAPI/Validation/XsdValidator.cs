using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace KS.FiksProtokollValidator.WebAPI.Validation
{
    public class XsdValidator
    {
        public static void ValidateArkivmeldingKvittering(string payload, List<string> validationErrors)
        {
            var validationHandler = new ValidationHandler();
            var xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.Schemas.Add("http://www.arkivverket.no/standarder/noark5/arkivmelding/v2", "Schema/arkivmelding.xsd");
            xmlReaderSettings.Schemas.Add("http://www.arkivverket.no/standarder/noark5/metadatakatalog/v2", "Schema/metadatakatalog.xsd");
            xmlReaderSettings.ValidationType = ValidationType.Schema;
            xmlReaderSettings.ValidationEventHandler +=
                new ValidationEventHandler(validationHandler.HandleValidationError);

            var xmlReader = XmlReader.Create(new StringReader(payload), xmlReaderSettings);

            while (xmlReader.Read())
            {
            }

            if (validationHandler.HasErrors())
            {
                validationErrors.AddRange(validationHandler.errors);
            }
        }
    }
}