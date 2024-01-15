using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using KS.Fiks.ASiC_E;
using KS.Fiks.IO.Client.Models;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Models;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Utilities.Validation;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.KlientValidator.Managers.FiksArkiv
{
    public class BaseManager
    {
        private readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
        protected readonly XmlSchemaSet XmlSchemaSet;

        protected BaseManager()
        {
            XmlSchemaSet = new XmlSchemaSet();
            var arkivModelsAssembly = Assembly.Load("KS.Fiks.Arkiv.Models.V1");
            
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.arkivering.arkivmelding.opprett.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    XmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/arkivmelding/opprett/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                   arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.arkivering.dokumentobjekt.opprett.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    XmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/dokumentobjekt/opprett/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.arkivering.arkivmelding.oppdater.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    XmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/arkivmelding/oppdater/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                   arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.arkivering.avskrivning.opprett.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    XmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/avskrivning/opprett/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                   arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.arkivering.avskrivning.slett.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    XmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/avskrivning/slett/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.innsyn.dokumentfil.hent.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    XmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/dokumentfil/hent/v1",
                        schemaReader);
                }
            }
            
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.innsyn.registrering.hent.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    XmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/registrering/hent/v1",
                        schemaReader);
                }
            }
            using (var schemaStream =
                arkivModelsAssembly.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.innsyn.mappe.hent.xsd"))
            {
                using (var schemaReader = XmlReader.Create(schemaStream))
                {
                    XmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/mappe/hent/v1",
                        schemaReader);
                }
            }
            using (var schemaStream = arkivModelsAssembly?.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.no.ks.fiks.arkiv.v1.innsyn.sok.xsd"))
            {
                if (schemaStream != null)
                {
                    using var schemaReader = XmlReader.Create(schemaStream);
                    XmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/sok/v1", schemaReader);
                }
            }
            using (var schemaStream = arkivModelsAssembly?.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.arkivstruktur.xsd"))
            {
                if (schemaStream != null)
                {
                    using var schemaReader = XmlReader.Create(schemaStream);
                    XmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/arkivstruktur/v1",
                        schemaReader);
                }
            }
            using (var schemaStream = arkivModelsAssembly?.GetManifestResourceStream("KS.Fiks.Arkiv.Models.V1.Schema.V1.metadatakatalog.xsd"))
            {
                if (schemaStream != null)
                {
                    using var schemaReader = XmlReader.Create(schemaStream);
                    XmlSchemaSet.Add("https://ks-no.github.io/standarder/fiks-protokoll/fiks-arkiv/metadatakatalog/v1",
                        schemaReader);
                }
            }
        }

        protected XmlReaderResult ValidateAndGetPayloadAsString(MottattMeldingArgs mottatt, XmlSchemaSet xmlSchemaSet)
        {
            var xmlReaderResult = new XmlReaderResult();
            
            IAsicReader reader = new AsiceReader();
            using (var inputStream = mottatt.Melding.DecryptedStream.Result)
            using (var asice = reader.Read(inputStream))
            {
                foreach (var asiceReadEntry in asice.Entries)
                {
                    using (var entryStream = asiceReadEntry.OpenStream())
                    {
                        if (asiceReadEntry.FileName.Contains(".xml"))
                        {
                            xmlReaderResult.validationMessages = new XmlValidator().ValidateXml(
                                entryStream,
                                xmlSchemaSet
                            );
                            if (xmlReaderResult.validationMessages != null && xmlReaderResult.validationMessages[0].Count > 0)
                            {
                                xmlReaderResult.XmlValidationErrorOccured = true;
                            }

                            var newEntryStream = asiceReadEntry.OpenStream();
                            var reader1 = new StreamReader(newEntryStream);
                            xmlReaderResult.Xml = reader1.ReadToEnd();
                        }
                    }

                    Log.Information("Mottatt vedlegg: {Filename}", asiceReadEntry.FileName);
                }
            }

            return xmlReaderResult;
        }
        
        
        protected XmlReaderResult ValidatePayload(MottattMeldingArgs mottatt, XmlSchemaSet xmlSchemaSet)
        {
            var xmlReaderResult = new XmlReaderResult();
            IAsicReader reader = new AsiceReader();
          
            using (var inputStream = mottatt.Melding.DecryptedStream.Result)
            using (var asice = reader.Read(inputStream))
            {
                foreach (var asiceReadEntry in asice.Entries)
                {
                    using (var entryStream = asiceReadEntry.OpenStream())
                    {
                        if (asiceReadEntry.FileName.Contains(".xml"))
                        {
                            xmlReaderResult.validationMessages = new XmlValidator().ValidateXml(
                                entryStream,
                                xmlSchemaSet
                            );
                            if (xmlReaderResult.validationMessages != null && xmlReaderResult.validationMessages[0].Count > 0)
                            {
                                xmlReaderResult.XmlValidationErrorOccured = true;
                            }
                        }
                    }
                    Log.Information("Mottatt vedlegg: {Filename}", asiceReadEntry.FileName);
                }
            }

            return xmlReaderResult;
        }
    }
}