using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using KS.Fiks.Arkiv.Models.V1.Arkivering.Arkivmelding;
using KS.Fiks.Arkiv.Models.V1.Arkivering.Arkivmeldingkvittering;
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
 
        public List<Melding> HandleMelding(MottattMeldingArgs mottatt)
        {
            var meldinger = new List<Melding>();
            
            Arkivmelding arkivmelding;
            if (mottatt.Melding.HasPayload)
            {
                var xmlReaderResult = ValidateAndGetPayloadAsString(mottatt, XmlSchemaSet);
                if (xmlReaderResult.XmlValidationErrorOccured) // Ugyldig forespørsel
                {
                    Log.Information($"Xml validering feilet: {xmlReaderResult.validationMessages}");
                    meldinger.Add(new Melding
                    {
                        ResultatMelding = FeilmeldingEngine.CreateUgyldigforespoerselMelding(xmlReaderResult.validationMessages),
                        FileName = "feilmelding.xml",
                        MeldingsType = FiksArkivMeldingtype.Ugyldigforespørsel,
                    });
                    return meldinger;
                }
                Log.Debug("{Kilde} - Parsing arkivmelding: {Xml}", GetType().Name, xmlReaderResult.Xml);
                if (string.IsNullOrEmpty(xmlReaderResult.Xml))
                {
                    Log.Error("Tom arkivmelding");
                }
                using var textReader = (TextReader) new StringReader(xmlReaderResult.Xml);
                arkivmelding = (Arkivmelding) new XmlSerializer(typeof(Arkivmelding)).Deserialize(textReader);
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

            ArkivmeldingKvittering kvittering;
            if (arkivmelding?.Mappe != null)
            {
                kvittering = ArkivmeldingKvitteringBuilder.Init().WithSaksmappe(arkivmelding.Mappe).Build();

            }
            else
            {
                kvittering = ArkivmeldingKvitteringBuilder.Init().WithJournalpost(arkivmelding?.Registrering).Build();    
            }

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