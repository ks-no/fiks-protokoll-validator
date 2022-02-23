using System.Collections.Generic;
using KS.Fiks.IO.Arkiv.Client.Models;
using KS.Fiks.IO.Client.Models.Feilmelding;
using KS.Fiks.IO.Politiskbehandling.Client.Models;
using KS.Fiks.Plan.Client.Models;

namespace KS.FiksProtokollValidator.WebAPI.Validation
{
    public static class PayloadChecksHelper
    {
        public static HashSet<string> GetMessageTypesWithPayload()
        { //NB Husk at man må fylle på i denne listen med de meldingstyper som har resultat.
            return new HashSet<string>
            {
                ArkivintegrasjonMeldingTypeV1.ArkivmeldingKvittering,
                ArkivintegrasjonMeldingTypeV1.SokResultatMinimum,
                ArkivintegrasjonMeldingTypeV1.SokResultatNoekler,
                ArkivintegrasjonMeldingTypeV1.SokResultatUtvidet,
                WebAPI.Resources.ResponseMessageTypes.FeilV1, //TODO er denne i bruk?
                PolitiskBehandlingMeldingTypeV1.ResultatMoeteplan,
                PolitiskBehandlingMeldingTypeV1.ResultatUtvalg,
                FeilmeldingMeldingTypeV1.Ugyldigforespørsel,
                FiksPlanMeldingtypeV2.ResultatFinnPlanerForMatrikkelenhet,
                FiksPlanMeldingtypeV2.ResultatFinnPlaner,
                FiksPlanMeldingtypeV2.ResultatFinnDispensasjoner,
                FiksPlanMeldingtypeV2.ResultatFinnPlanbehandlinger,
                FiksPlanMeldingtypeV2.ResultatOpprettArealplan,
                FiksPlanMeldingtypeV2.ResultatHentAktoerer,
                FiksPlanMeldingtypeV2.ResultatHentArealplan,
                FiksPlanMeldingtypeV2.ResultatHentBboxForPlan,
                FiksPlanMeldingtypeV2.ResultatHentRelatertePlaner,
                FiksPlanMeldingtypeV2.ResultatHentGjeldendePlanbestemmelser,
                FiksPlanMeldingtypeV2.ResultatHentKodeliste,
                FiksPlanMeldingtypeV2.ResultatFinnPlandokumenter,
                FiksPlanMeldingtypeV2.OpprettArealplan,
                FiksPlanMeldingtypeV2.ResultatHentPlanomraader,
                FiksPlanMeldingtypeV2.ResultatFinnPlanerForOmraade,
                FiksPlanMeldingtypeV2.ResultatSjekkMidlertidigForbud,
            };
        }

        public static string GetExpectedFileName(string messageType)
        {
            switch (messageType)
            {
                case ArkivintegrasjonMeldingTypeV1.Arkivmelding:
                    return "arkivmelding.xml";
                case ArkivintegrasjonMeldingTypeV1.ArkivmeldingKvittering:
                    return "arkivmelding-kvittering.xml";
                case ArkivintegrasjonMeldingTypeV1.Sok:
                    return "sok.xml";
                case ArkivintegrasjonMeldingTypeV1.SokResultatMinimum:
                    return "sokeresultat-minimum.xml";
                case ArkivintegrasjonMeldingTypeV1.SokResultatNoekler:
                    return "sokeresultat-noekler.xml";
                case ArkivintegrasjonMeldingTypeV1.SokResultatUtvidet:
                    return "sokeresultat-utvidet.xml";
                case ArkivintegrasjonMeldingTypeV1.DokumentfilHent:
                case ArkivintegrasjonMeldingTypeV1.DokumentfilHentResultat:
                case ArkivintegrasjonMeldingTypeV1.MappeHent:
                case ArkivintegrasjonMeldingTypeV1.MappeHentResultat:
                case ArkivintegrasjonMeldingTypeV1.JournalpostHent:
                case ArkivintegrasjonMeldingTypeV1.JournalpostHentResultat:
                    return "arkivmelding.xml";
                case PolitiskBehandlingMeldingTypeV1.HentMoeteplan:
                case PolitiskBehandlingMeldingTypeV1.HentUtvalg:
                case PolitiskBehandlingMeldingTypeV1.SendOrienteringssak:
                case PolitiskBehandlingMeldingTypeV1.SendUtvalgssak:
                case PolitiskBehandlingMeldingTypeV1.SendDelegertVedtak:
                case PolitiskBehandlingMeldingTypeV1.SendVedtakFraUtvalg:
                case PolitiskBehandlingMeldingTypeV1.SendMoeteplanTilEInnsyn:
                case PolitiskBehandlingMeldingTypeV1.SendUtvalgssakerTilEInnsyn:
                case PolitiskBehandlingMeldingTypeV1.SendVedtakTilEInnsyn:
                case PolitiskBehandlingMeldingTypeV1.ResultatMoeteplan:
                case PolitiskBehandlingMeldingTypeV1.ResultatUtvalg:
                case FeilmeldingMeldingTypeV1.Ugyldigforespørsel:
                case FeilmeldingMeldingTypeV1.Serverfeil:
                case FiksPlanMeldingtypeV2.ResultatFinnDispensasjoner:
                case FiksPlanMeldingtypeV2.ResultatFinnPlanbehandlinger:
                case FiksPlanMeldingtypeV2.ResultatFinnPlandokumenter:
                case FiksPlanMeldingtypeV2.ResultatFinnPlanerForMatrikkelenhet:
                case FiksPlanMeldingtypeV2.ResultatFinnPlaner:
                case FiksPlanMeldingtypeV2.ResultatHentAktoerer:
                case FiksPlanMeldingtypeV2.ResultatHentArealplan:
                case FiksPlanMeldingtypeV2.ResultatHentBboxForPlan:
                case FiksPlanMeldingtypeV2.ResultatHentGjeldendePlanbestemmelser:
                case FiksPlanMeldingtypeV2.ResultatHentKodeliste:
                case FiksPlanMeldingtypeV2.ResultatHentPlanomraader:
                case FiksPlanMeldingtypeV2.ResultatHentRelatertePlaner:
                case FiksPlanMeldingtypeV2.ResultatOpprettArealplan:
                case FiksPlanMeldingtypeV2.ResultatFinnPlanerForOmraade:
                case FiksPlanMeldingtypeV2.ResultatSjekkMidlertidigForbud:
                    return "payload.json";
                default:
                    return string.Empty;
            }
        }

        public static bool HasValidFileFormat(string receivedPayloadFileName)
        {
            return receivedPayloadFileName.EndsWith(".xml") ||
                   receivedPayloadFileName.EndsWith(".json") ||
                   receivedPayloadFileName.EndsWith(".txt");
        }
    }
}