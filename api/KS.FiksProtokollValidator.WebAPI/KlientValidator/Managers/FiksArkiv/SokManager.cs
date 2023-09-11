using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Schema;
using System.Xml.Serialization;
using KS.Fiks.Arkiv.Models.V1.Innsyn.Sok;
using KS.Fiks.Arkiv.Models.V1.Meldingstyper;
using KS.Fiks.IO.Client.Models;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Engines.FiksArkiv;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Models;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.KlientValidator.Managers.FiksArkiv
{
    public class SokManager : BaseManager
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
        
        public SokManager()
        {
        }

        private Sok GetPayload(MottattMeldingArgs mottatt, XmlSchemaSet xmlSchemaSet,
            out bool xmlValidationErrorOccured, out List<List<string>>? validationResult)
        {
            if (mottatt.Melding.HasPayload)
            {
                // Verify that message has payload
                var text = GetPayloadAsString(mottatt, xmlSchemaSet, out xmlValidationErrorOccured, out validationResult);
                Log.Debug("{Kilde} - Parsing sok: {SokText}", GetType().Name, text);
                if (string.IsNullOrEmpty(text))
                {
                    Log.Error("Tom sok? Text: {Sok}", text);
                }

                using var textReader = (TextReader)new StringReader(text);
                return (Sok)new XmlSerializer(typeof(Sok)).Deserialize(textReader);
            }

            xmlValidationErrorOccured = false;
            validationResult = null;
            return null;
        }
        
        public Melding HandleMelding(MottattMeldingArgs mottatt)
        {
            var sok = GetPayload(mottatt, XmlSchemaSet, out var xmlValidationErrorOccured,
                out var validationResult);

            if (xmlValidationErrorOccured)
            {
                return new Melding
                {
                    ResultatMelding = FeilmeldingEngine.CreateUgyldigforespoerselMelding(validationResult),
                    FileName = "feilmelding.xml",
                    MeldingsType = FiksArkivMeldingtype.Ugyldigforespørsel,
                };
            }

            return SokGenerator.CreateSokResponseMelding(sok);
        }
    }
}