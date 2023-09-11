using System.Collections.Generic;
using System.Reflection;
using KS.Fiks.Arkiv.Models.V1.Meldingstyper;
using KS.Fiks.IO.Client.Models;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Engines.FiksArkiv;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Models;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.KlientValidator.Managers.FiksArkiv
{
    public class ArkivmeldingOppdaterManager : BaseManager
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);

        public List<Melding> HandleMelding(MottattMeldingArgs mottatt)
        {
            var meldinger = new List<Melding>();

            if (!mottatt.Melding.HasPayload) // Skal ha payload!
            {
                meldinger.Add(new Melding
                {
                    ResultatMelding =
                        FeilmeldingEngine.CreateUgyldigforespoerselMelding(
                            "ArkivmeldingOppdatering meldingen mangler innhold som er påkrevd"),
                    FileName = "feilmelding.xml",
                    MeldingsType = FiksArkivMeldingtype.Ugyldigforespørsel,
                });
                return meldinger;
            }

            var validationResult = ValidatePayload(mottatt, XmlSchemaSet);

            if (validationResult.Count > 0) // Ugyldig forespørsel pga valideringsfeil
            {
                meldinger.Add(new Melding
                {
                    ResultatMelding = FeilmeldingEngine.CreateUgyldigforespoerselMelding(validationResult),
                    FileName = "feilmelding.xml",
                    MeldingsType = FiksArkivMeldingtype.Ugyldigforespørsel,
                });
                return meldinger;
            }

            // Melding er validert i henhold til xsd, vi sender først tilbake en tom mottatt melding
            meldinger.Add(new Melding
            {
                MeldingsType = FiksArkivMeldingtype.ArkivmeldingOppdaterMottatt
            });

            // Så sender vi tilbake en tom kvitteringsmelding
            meldinger.Add(new Melding
            {
                MeldingsType = FiksArkivMeldingtype.ArkivmeldingOppdaterKvittering
            });

            return meldinger;
        }
    }
}