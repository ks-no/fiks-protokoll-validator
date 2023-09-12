using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Schema;
using System.Xml.Serialization;
using KS.Fiks.Arkiv.Models.V1.Arkivering.Arkivmelding;
using KS.Fiks.Arkiv.Models.V1.Meldingstyper;
using KS.Fiks.IO.Client.Models;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Engines.FiksArkiv;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Models;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.KlientValidator.Managers.FiksArkiv
{
    public class ArkivmeldingManager : BaseManager
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
        
        private Arkivmelding GetPayload(MottattMeldingArgs mottatt, XmlSchemaSet xmlSchemaSet,
            out bool xmlValidationErrorOccured, out List<List<string>>? validationResult)
        {
            if (mottatt.Melding.HasPayload)
            {
                var text = GetPayloadAsString(mottatt, xmlSchemaSet, out xmlValidationErrorOccured,
                    out validationResult);
                Log.Debug("{Kilde} - Parsing arkivmelding: {Xml}", GetType().Name, text);
                if (string.IsNullOrEmpty(text))
                {
                    Log.Error("Tom arkivmelding? Xml: {Xml}", text);
                }

                using var textReader = (TextReader)new StringReader(text);
                return (Arkivmelding) new XmlSerializer(typeof(Arkivmelding)).Deserialize(textReader);
            }

            xmlValidationErrorOccured = false;
            validationResult = null;
            return null;
        }
        
        public List<Melding> HandleMelding(MottattMeldingArgs mottatt)
        {
            var meldinger = new List<Melding>();
            
            Arkivmelding arkivmelding;
            if (mottatt.Melding.HasPayload)
            {
                arkivmelding = GetPayload(mottatt, XmlSchemaSet,
                    out var xmlValidationErrorOccured, out var validationResult);

                if (xmlValidationErrorOccured) // Ugyldig forespørsel
                {
                    Log.Information($"Xml validering feilet: {validationResult}");
                    meldinger.Add(new Melding
                    {
                        ResultatMelding = FeilmeldingEngine.CreateUgyldigforespoerselMelding(validationResult),
                        FileName = "feilmelding.xml",
                        MeldingsType = FiksArkivMeldingtype.Ugyldigforespørsel,
                    });
                    return meldinger;
                }
            }
            else // Missing payload
            {
                meldinger.Add(new Melding
                {
                    ResultatMelding =
                        FeilmeldingEngine.CreateUgyldigforespoerselMelding("Arkivmelding meldingen mangler innhold"),
                    FileName = "feilmelding.xml",
                    MeldingsType = FiksArkivMeldingtype.Ugyldigforespørsel,
                });
                return meldinger;
            }

            var kvittering = ArkivmeldingKvitteringEngine.CreateArkivmeldingKvittering(arkivmelding);
            
            
            // Det skal først sendes en tom mottatt-melding
            meldinger.Add(new Melding
            {
                MeldingsType = FiksArkivMeldingtype.ArkivmeldingOpprettMottatt
            });
            
            // Så sendes en kvitteringsmelding med payload
            meldinger.Add(new Melding
            {
                ResultatMelding = kvittering,
                FileName = "arkivmelding-kvittering.xml",
                MeldingsType = FiksArkivMeldingtype.ArkivmeldingOpprettKvittering
            });
            
            return meldinger;
        }
    }
}