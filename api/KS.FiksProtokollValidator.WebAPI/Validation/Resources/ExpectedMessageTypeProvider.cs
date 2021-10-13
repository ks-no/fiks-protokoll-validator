using System.Collections.Generic;
using KS.Fiks.IO.Politiskbehandling.Client.Models;
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
                    WebAPI.Resources.RequestMessageTypes.ArkivmeldingV1,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattV1,
                        WebAPI.Resources.ResponseMessageTypes.KvitteringV1
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.OppdaterSaksmappeV1,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattV1,
                        WebAPI.Resources.ResponseMessageTypes.KvitteringV1
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.SoekV1,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.InnsynSoekResultatV1
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
                        PolitiskBehandlingMeldingTypeV1.Mottatt
                    }
                },
                {
                    PolitiskBehandlingMeldingTypeV1.SendOrienteringssak,
                    new List<string>
                    {
                        PolitiskBehandlingMeldingTypeV1.Mottatt
                    }
                },
                {
                    PolitiskBehandlingMeldingTypeV1.SendDelegertVedtak,
                    new List<string>
                    {
                        PolitiskBehandlingMeldingTypeV1.Mottatt
                    }
                },
                {
                    WebAPI.Resources.RequestMessageTypes.SendVedtakFraUtvalgV1,
                    new List<string>
                    {
                        WebAPI.Resources.ResponseMessageTypes.MottattPolitiskBehandlingV1
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
