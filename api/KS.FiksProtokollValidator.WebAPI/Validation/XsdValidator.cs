using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace KS.FiksProtokollValidator.WebAPI.Validation
{
    public class XsdValidator
    {
        private string baseDirectory;
        
        public XsdValidator()
        {
            baseDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        } 
        
        public XsdValidator(string baseDirectory)
        {
            this.baseDirectory = baseDirectory;
        } 
        
        private void Validate(string payload, List<string> validationErrors, XmlReaderSettings xmlReaderSettings)
        {
            var validationHandler = new ValidationHandler();
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
        
        public void ValidateArkivmeldingKvittering(string payload, List<string> validationErrors)
        {
           
            
            var xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.Schemas.Add("http://www.arkivverket.no/standarder/noark5/arkivmelding/v2",
                Path.Combine(baseDirectory, "Schema/arkivmelding.xsd"));//"Schema/arkivmelding.xsd");
            xmlReaderSettings.Schemas.Add("http://www.arkivverket.no/standarder/noark5/metadatakatalog/v2",
                Path.Combine(baseDirectory, "Schema/metadatakatalog.xsd"));
            Validate(payload, validationErrors, xmlReaderSettings);
        }
        
        public void ValidateArkivmeldingSokeresultatMinimum(string payload, List<string> validationErrors)
        {
            var xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.Schemas.Add("http://www.ks.no/standarder/fiks/arkiv/sokeresultat/v1",
                Path.Combine(baseDirectory, "Schema/sokeresultatMinimum.xsd"));
            xmlReaderSettings.Schemas.Add("http://www.ks.no/standarder/fiks/arkiv/arkivstruktur/minimum/v1",
                Path.Combine(baseDirectory, "Schema/arkivstrukturMinimum.xsd"));
            xmlReaderSettings.Schemas.Add("http://www.arkivverket.no/standarder/noark5/metadatakatalog/v2",
                Path.Combine(baseDirectory, "Schema/metadatakatalog.xsd"));
            Validate(payload, validationErrors, xmlReaderSettings);
        }
        
        public void ValidateArkivmeldingSokeresultatNoekler(string payload, List<string> validationErrors)
        {
            var xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.Schemas.Add("http://www.ks.no/standarder/fiks/arkiv/sokeresultat/v1",
                Path.Combine(baseDirectory, "Schema/sokeresultatNoekler.xsd"));
            xmlReaderSettings.Schemas.Add("http://www.ks.no/standarder/fiks/arkiv/arkivstruktur/noekler/v1",
                Path.Combine(baseDirectory, "Schema/arkivstrukturNoekler.xsd"));
            xmlReaderSettings.Schemas.Add("http://www.arkivverket.no/standarder/noark5/metadatakatalog/v2",
                Path.Combine(baseDirectory, "Schema/metadatakatalog.xsd"));
            Validate(payload, validationErrors, xmlReaderSettings);
        }

        public void ValidateArkivmeldingSokeresultatUtvidet(string payload, List<string> validationErrors)
        {
            var xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.Schemas.Add("http://www.ks.no/standarder/fiks/arkiv/sokeresultat/v1",
                Path.Combine(baseDirectory,"Schema/sokeresultatUtvidet.xsd"));
            xmlReaderSettings.Schemas.Add("http://www.arkivverket.no/standarder/noark5/arkivstruktur",
                Path.Combine(baseDirectory, "Schema/arkivstruktur.xsd"));   
            xmlReaderSettings.Schemas.Add("http://www.arkivverket.no/standarder/noark5/metadatakatalog/v2",
                Path.Combine(baseDirectory, "Schema/metadatakatalog.xsd"));
            Validate(payload, validationErrors, xmlReaderSettings);
        }
    }
}