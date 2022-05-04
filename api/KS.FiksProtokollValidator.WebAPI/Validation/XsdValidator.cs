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
        private XmlSchemaSet _xmlSchemaSet;
        private XmlReaderSettings xmlReaderSettings;
        
        public XsdValidator()
        {
            InitXmlSchemaSets();
        }

        private void InitXmlSchemaSets()
        {
            _xmlSchemaSet = new XmlSchemaSet();

            var arkivModelsAssembly = Assembly
                .GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Select(a => Assembly.Load(a.FullName)).SingleOrDefault(assembly => assembly.GetName().Name == "KS.Fiks.Arkiv.Models.V1"); //AppDomain.CurrentDomain.GetAssemblies()

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.arkivmelding.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("http://www.arkivverket.no/standarder/noark5/arkivmelding/v2",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.metadatakatalog.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("http://www.arkivverket.no/standarder/noark5/metadatakatalog/v2",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.sokeresultatMinimum.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("http://www.ks.no/standarder/fiks/arkiv/sokeresultat/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.arkivstrukturMinimum.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("http://www.ks.no/standarder/fiks/arkiv/arkivstruktur/minimum/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.sokeresultatNoekler.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("http://www.ks.no/standarder/fiks/arkiv/sokeresultat/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.arkivstrukturNoekler.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("http://www.ks.no/standarder/fiks/arkiv/arkivstruktur/noekler/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.sokeresultatUtvidet.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("http://www.ks.no/standarder/fiks/arkiv/sokeresultat/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.arkivstruktur.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("http://www.arkivverket.no/standarder/noark5/arkivstruktur",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.journalpostHent.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("http://www.arkivverket.no/standarder/noark5/journalpost/hent/v2",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.journalpostHentResultat.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("http://www.arkivverket.no/standarder/noark5/journalpost/hent/resultat/v2",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.mappeHent.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("http://www.arkivverket.no/standarder/noark5/mappe/hent/v2",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.mappeHentResultat.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("http://www.arkivverket.no/standarder/noark5/mappe/hent/resultat/v2",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.dokumentfilHent.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("http://www.arkivverket.no/standarder/noark5/dokumentfil/hent/v2",
                        schemaReader);
                }
            }
            
            xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.Schemas.Add(_xmlSchemaSet);
        }

        public void Validate(string payload, List<string> validationErrors)
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
    }
}