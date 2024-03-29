using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation
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
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.arkivering.arkivmelding.opprett.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/arkivmelding/opprett/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                   arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.arkivering.arkivmelding.oppdater.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/arkivmelding/oppdater/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                   arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.arkivering.avskrivning.opprett.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/avskrivning/opprett/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                   arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.arkivering.avskrivning.slett.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/avskrivning/slett/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                   arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.arkivering.dokumentobjekt.opprett.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/dokumentobjekt/opprett/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.metadatakatalog.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/metadatakatalog/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                   arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.innsyn.sok.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/sok/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.innsyn.sok.resultat.minimum.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/sokeresultat/minimum/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.arkivstrukturMinimum.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/arkivstruktur/minimum/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.innsyn.sok.resultat.noekler.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/sokeresultat/noekler/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.arkivstrukturNoekler.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/arkivstruktur/noekler/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.innsyn.sok.resultat.utvidet.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/sokeresultat/utvidet/v1",
                        schemaReader);
                }
            }

            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.arkivstruktur.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/arkivstruktur/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.innsyn.registrering.hent.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/registrering/hent/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.innsyn.registrering.hent.resultat.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/registrering/hent/resultat/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.innsyn.mappe.hent.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/mappe/hent/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.innsyn.mappe.hent.resultat.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/mappe/hent/resultat/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.innsyn.dokumentfil.hent.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    _xmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/dokumentfil/hent/v1",
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

            try
            {
                while (xmlReader.Read())
                {
                }
            }
            catch (Exception e)
            {
                validationErrors.Add($"Validating the xml failed. {e.Message}");
            }
            if (validationHandler.HasErrors())
            {
                validationErrors.AddRange(validationHandler.errors);
            }
        }
    }
}