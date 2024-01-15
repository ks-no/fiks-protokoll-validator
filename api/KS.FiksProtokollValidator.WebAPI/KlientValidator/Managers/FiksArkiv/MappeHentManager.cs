using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Schema;
using System.Xml.Serialization;
using KS.Fiks.Arkiv.Models.V1.Innsyn.Hent.Mappe;
using KS.Fiks.Arkiv.Models.V1.Meldingstyper;
using KS.Fiks.IO.Client.Models;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Engines.FiksArkiv;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Models;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.KlientValidator.Managers.FiksArkiv
{
    public class MappeHentManager : BaseManager
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);

        public Melding HandleMelding(MottattMeldingArgs mottatt)
        {
            if (!mottatt.Melding.HasPayload)
            {
                return new Melding
                {
                    ResultatMelding =
                        FeilmeldingEngine.CreateUgyldigforespoerselMelding("Hent mappe meldingen mangler innhold"),
                    FileName = "feilmelding.xml",
                    MeldingsType = FiksArkivMeldingtype.Ugyldigforespørsel,
                };
            }

            var xmlReaderResult = ValidateAndGetPayloadAsString(mottatt, XmlSchemaSet);

            if (xmlReaderResult.XmlValidationErrorOccured) // Ugyldig forespørsel
            {
                Log.Information($"Xml validering feilet: {xmlReaderResult.validationMessages}");
                return new Melding
                {
                    ResultatMelding = FeilmeldingEngine.CreateUgyldigforespoerselMelding(xmlReaderResult.validationMessages),
                    FileName = "feilmelding.xml",
                    MeldingsType = FiksArkivMeldingtype.Ugyldigforespørsel,
                };
            }
            Log.Debug("{Kilde} - Parsing arkivmelding: {Xml}", GetType().Name, xmlReaderResult.Xml);
            if (string.IsNullOrEmpty(xmlReaderResult.Xml))
            {
                Log.Error("Tom arkivmelding");
            }
            using var textReader = (TextReader)new StringReader(xmlReaderResult.Xml);
            var hentMelding = (MappeHent) new XmlSerializer(typeof(MappeHent)).Deserialize(textReader);
            return new Melding(){
                ResultatMelding = SaksmappeHentResultatBuilder.Init().WithSaksmappe(hentMelding).Build(),
                FileName = "resultat.xml",
                MeldingsType = FiksArkivMeldingtype.MappeHentResultat
            };
        }
    }
}