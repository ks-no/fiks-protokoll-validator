using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace KS.FiksProtokollValidator.WebAPI.Validation
{
    public class XsdValidator
    {
        private string baseDirectory;
        private XmlSchemaSet _arkivmeldingKvitteringXmlSchemaSet;
        private XmlSchemaSet _sokeresultatMinimumXmlSchemaSet;
        private XmlSchemaSet _sokeresultatNoeklerXmlSchemaSet;
        private XmlSchemaSet _sokeresultatUtvidetXmlSchemaSet;
        private XmlSchemaSet _metadatakatalogXmlSchemaSet;
        
        
        public XsdValidator()
        {
            baseDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            InitXmlSchemaSets();
        }

        private void InitXmlSchemaSets()
        {
            _arkivmeldingKvitteringXmlSchemaSet = new XmlSchemaSet();
            _sokeresultatMinimumXmlSchemaSet = new XmlSchemaSet();
            _sokeresultatNoeklerXmlSchemaSet = new XmlSchemaSet();
            _sokeresultatUtvidetXmlSchemaSet = new XmlSchemaSet();
            _metadatakatalogXmlSchemaSet = new XmlSchemaSet();

            var arkivModelsAssembly = Assembly
                .GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Select(a => Assembly.Load(a.FullName)).SingleOrDefault(assembly => assembly.GetName().Name == "KS.Fiks.Arkiv.Models.V1"); //AppDomain.CurrentDomain.GetAssemblies()
                // .SingleOrDefault(assembly => assembly.GetName().Name == "KS.Fiks.Arkiv.Models.V1");

            // Arkivmelding kvittering
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.arkivmelding.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _arkivmeldingKvitteringXmlSchemaSet.Add("http://www.arkivverket.no/standarder/noark5/arkivmelding/v2",
                        schemaReader);
                }
            }

            // Metadatakatalog felles
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.metadatakatalog.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _metadatakatalogXmlSchemaSet.Add("http://www.arkivverket.no/standarder/noark5/metadatakatalog/v2",
                        schemaReader);
                }
            }

            // Sokeresultat minimum
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.sokeresultatMinimum.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _sokeresultatMinimumXmlSchemaSet.Add("http://www.ks.no/standarder/fiks/arkiv/sokeresultat/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.arkivstrukturMinimum.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _sokeresultatMinimumXmlSchemaSet.Add("http://www.ks.no/standarder/fiks/arkiv/arkivstruktur/minimum/v1",
                        schemaReader);
                }
            }

            // Sokeresultat noekler
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.sokeresultatNoekler.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _sokeresultatNoeklerXmlSchemaSet.Add("http://www.ks.no/standarder/fiks/arkiv/sokeresultat/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.arkivstrukturNoekler.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _sokeresultatNoeklerXmlSchemaSet.Add("http://www.ks.no/standarder/fiks/arkiv/arkivstruktur/noekler/v1",
                        schemaReader);
                }
            }

            // Sokeresultat utvidet
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.sokeresultatUtvidet.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _sokeresultatUtvidetXmlSchemaSet.Add("http://www.ks.no/standarder/fiks/arkiv/sokeresultat/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.arkivstruktur.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _sokeresultatUtvidetXmlSchemaSet.Add("http://www.arkivverket.no/standarder/noark5/arkivstruktur",
                        schemaReader);
                }
            }
        }

        public XsdValidator(string baseDirectory)
        {
            this.baseDirectory = baseDirectory;
            InitXmlSchemaSets();
        } 
        
        private void Validate(string payload, List<string> validationErrors, XmlReaderSettings xmlReaderSettings)
        {
            var validationHandler = new ValidationHandler();
            xmlReaderSettings.ValidationType = ValidationType.Schema;
            xmlReaderSettings.ValidationEventHandler += validationHandler.HandleValidationError;

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
            xmlReaderSettings.Schemas.Add(_arkivmeldingKvitteringXmlSchemaSet);
            xmlReaderSettings.Schemas.Add(_metadatakatalogXmlSchemaSet);
            Validate(payload, validationErrors, xmlReaderSettings);
        }
        
        public void ValidateArkivmeldingSokeresultatMinimum(string payload, List<string> validationErrors)
        {
            var xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.Schemas.Add(_sokeresultatMinimumXmlSchemaSet);
            xmlReaderSettings.Schemas.Add(_metadatakatalogXmlSchemaSet);
            Validate(payload, validationErrors, xmlReaderSettings);
        }
        
        public void ValidateArkivmeldingSokeresultatNoekler(string payload, List<string> validationErrors)
        {
            var xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.Schemas.Add(_sokeresultatNoeklerXmlSchemaSet);         
            xmlReaderSettings.Schemas.Add(_metadatakatalogXmlSchemaSet);         
            Validate(payload, validationErrors, xmlReaderSettings);
        }

        public void ValidateArkivmeldingSokeresultatUtvidet(string payload, List<string> validationErrors)
        {
            var xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.Schemas.Add(_sokeresultatUtvidetXmlSchemaSet);
            xmlReaderSettings.Schemas.Add(_metadatakatalogXmlSchemaSet);
            Validate(payload, validationErrors, xmlReaderSettings);
        }
    }
}