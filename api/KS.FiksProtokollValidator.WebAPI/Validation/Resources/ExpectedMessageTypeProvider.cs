using System.Collections.Generic;
using KS.Fiks.IO.Politiskbehandling.Client.Models;
using KS.Fiks.IO.Arkiv.Client.Models;
using KS.FiksProtokollValidator.WebAPI.Models;

namespace KS.FiksProtokollValidator.WebAPI.Validation.Resources
{
    public static class ExpectedResponseMessageTypeProvider
    {
        private static Dictionary<string, List<string>> _expectedMessageTypes;

        private static Dictionary<string, List<string>> InitializeExpectedMessageTypes()
        {
            return new Dictionary<string, List<string>>
            {
                {
                    ArkivintegrasjonMeldingTypeV1.Arkivmelding,
                    new List<string>
                    {
                        ArkivintegrasjonMeldingTypeV1.ArkivmeldingMottatt,
                        ArkivintegrasjonMeldingTypeV1.ArkivmeldingKvittering
                    }
                },
                {
                    ArkivintegrasjonMeldingTypeV1.Sok,
                    new List<string>
                    {
                        ArkivintegrasjonMeldingTypeV1.SokResultatMinimum,
                        ArkivintegrasjonMeldingTypeV1.SokResultatNoekler,
                        ArkivintegrasjonMeldingTypeV1.SokResultatUtvidet
                    }
                },
                {
                    PolitiskBehandlingMeldingTypeV1.HentMoeteplan,
                    new List<string>
                    {
                        PolitiskBehandlingMeldingTypeV1.ResultatMoeteplan
                    }
                },
                {
                    PolitiskBehandlingMeldingTypeV1.HentUtvalg,
                    new List<string>
                    {
                        PolitiskBehandlingMeldingTypeV1.ResultatUtvalg
                    }
                },
                {
                    PolitiskBehandlingMeldingTypeV1.SendUtvalgssak,
                    new List<string>
                    {
                        PolitiskBehandlingMeldingTypeV1.SendUtvalgssakKvittering
                    }
                },
                {
                    PolitiskBehandlingMeldingTypeV1.SendOrienteringssak,
                    new List<string>
                    {
                        PolitiskBehandlingMeldingTypeV1.SendOrienteringssakKvittering
                    }
                },
                {
                    PolitiskBehandlingMeldingTypeV1.SendDelegertVedtak,
                    new List<string>
                    {
                        PolitiskBehandlingMeldingTypeV1.SendDelegertVedtakKvittering
                    }
                },
                {
                    PolitiskBehandlingMeldingTypeV1.SendVedtakFraUtvalg,
                    new List<string>
                    {
                        PolitiskBehandlingMeldingTypeV1.SendVedtakFraUtvalgKvittering
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.FinnPlanerForMatrikkelEnhetV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.FinnPlanerForMatrikkelenhetV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.RegistrerDispensasjonForPlanV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattGiPlanV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.RegistrerPlanavgrensingV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattGiPlanV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.FinnPlanerV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.FinnPlanerV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.FinnDispensasjonerV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.FinnDispensasjonerForSøkV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.OpprettArealplanV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.MeldingOmPlanidentV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.RegistrerPlanbehandlingV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattGiPlanV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.OppdaterArealplanV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattGiPlanV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.HentAktørerV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.AktørerV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.HentBboxV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.BboxV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.HentKodelisteV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.KodelisteV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.HentPlanfilV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.PlanfilV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.HentPlanområderV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.PlanområderV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.HentRelatertePlanerV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.RelatertePlanerV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.HentGjeldendePlanbestemmelserV2,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.GjeldendePlanbestemmelserV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.OppdaterDispensasjonV2,
                    new List<string>()
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattGiPlanV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.RegistrerMidlertidigForbudMotTiltakV2,
                    new List<string>()
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattGiPlanV2
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.FinnPlandokumenterV2,
                    new List<string>()
                    {
                        WebAPI.Resources.ResponseMessageTypes.PlandokumenterV2
                    }
                }
            };
        }

        public static List<string> GetExpectedResponseMessageTypesAsStrings(string requestMessageType,
            List<FiksExpectedResponseMessageType> testCaseExpectedResponseMessageTypes)
        {
            if (testCaseExpectedResponseMessageTypes.Count == 0)
            {
                _expectedMessageTypes ??= InitializeExpectedMessageTypes();

                return _expectedMessageTypes[requestMessageType];
            }

            var responseMessageTypesList = new List<string>();
            foreach (var responseMessageType in testCaseExpectedResponseMessageTypes)
            {
                responseMessageTypesList.Add(responseMessageType.ExpectedResponseMessageType);
            }

            return responseMessageTypesList;
        }
    }
}
