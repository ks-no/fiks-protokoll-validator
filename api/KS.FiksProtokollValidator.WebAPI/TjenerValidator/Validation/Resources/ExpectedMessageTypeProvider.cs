using System.Collections.Generic;
using KS.Fiks.Arkiv.Models.V1.Meldingstyper;
using KS.Fiks.IO.Politiskbehandling.Client.Models;
using KS.Fiks.Plan.Client.Models;
using KS.FiksProtokollValidator.WebAPI.Resources;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models;

namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation.Resources
{
    public static class ExpectedResponseMessageTypeProvider
    {
        private static Dictionary<string, List<string>> _expectedMessageTypes;

        private static Dictionary<string, List<string>> InitializeExpectedMessageTypes()
        {
            return new Dictionary<string, List<string>>
            {
                {
                    FiksArkivMeldingtype.ArkivmeldingOpprett,
                    new List<string>
                    {
                        FiksArkivMeldingtype.ArkivmeldingOpprettMottatt,
                        FiksArkivMeldingtype.ArkivmeldingOpprettKvittering
                    }
                },
                {
                    FiksArkivMeldingtype.ArkivmeldingOppdater,
                    new List<string>
                    {
                        FiksArkivMeldingtype.ArkivmeldingOppdaterMottatt,
                        FiksArkivMeldingtype.ArkivmeldingOppdaterKvittering
                    }
                },
                {
                    FiksArkivMeldingtype.AvskrivningOpprett,
                    new List<string>
                    {
                        FiksArkivMeldingtype.AvskrivningOpprettMottatt,
                        FiksArkivMeldingtype.AvskrivningOpprettKvittering
                    }
                },
                {
                    FiksArkivMeldingtype.AvskrivningSlett,
                    new List<string>
                    {
                        FiksArkivMeldingtype.AvskrivningSlettMottatt,
                        FiksArkivMeldingtype.AvskrivningSlettKvittering
                    }
                },
                {
                    FiksArkivMeldingtype.DokumentobjektOpprett,
                    new List<string>
                    {
                        FiksArkivMeldingtype.DokumentobjektOpprettMottatt,
                        FiksArkivMeldingtype.DokumentobjektOpprettKvittering
                    }
                },
                {
                    FiksArkivMeldingtype.Sok,
                    new List<string>
                    {
                        FiksArkivMeldingtype.SokResultatMinimum,
                        FiksArkivMeldingtype.SokResultatNoekler,
                        FiksArkivMeldingtype.SokResultatUtvidet
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
                        FiksPlanMeldingtypeV2.FinnPlanerForMatrikkelenhet,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatFinnPlanerForMatrikkelenhet
                    }
                },
                {
                    FiksPlanMeldingtypeV2.RegistrerDispensasjonFraPlan,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatMottat
                    }
                },
                {
                    FiksPlanMeldingtypeV2.RegistrertPlanavgrensning,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatMottat
                    }
                },
                {
                    FiksPlanMeldingtypeV2.FinnPlaner,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatFinnPlaner
                    }
                },
                {
                    FiksPlanMeldingtypeV2.FinnDispensasjoner,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatFinnDispensasjoner
                    }
                },
                {
                    FiksPlanMeldingtypeV2.OpprettArealplan,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatOpprettArealplan
                    }
                },
                {
                    FiksPlanMeldingtypeV2.RegistrerPlanbehandling,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatMottat
                    }
                },
                {
                    FiksPlanMeldingtypeV2.OppdaterArealplan,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatMottat
                    }
                },
                {
                    FiksPlanMeldingtypeV2.HentAktoerer,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatHentAktoerer
                    }
                },
                {
                    FiksPlanMeldingtypeV2.HentBboxForPlan,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatHentBboxForPlan
                    }
                },
                {
                    FiksPlanMeldingtypeV2.HentKodeliste,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatHentKodeliste
                    }
                },
                {
                    FiksPlanMeldingtypeV2.HentPlanfil,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatHentPlanfil
                    }
                },
                {
                    FiksPlanMeldingtypeV2.HentPlanomraader,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatHentPlanomraader
                    }
                },
                {
                    FiksPlanMeldingtypeV2.HentRelatertePlaner,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatHentRelatertePlaner
                    }
                },
                {
                    FiksPlanMeldingtypeV2.HentGjeldendePlanbestemmelser,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatHentGjeldendePlanbestemmelser
                    }
                },
                {
                    FiksPlanMeldingtypeV2.OppdaterDispensasjon,
                    new List<string>()
                    {
                        FiksPlanMeldingtypeV2.ResultatMottat
                    }
                },
                {
                    RequestMessageTypes.RegistrerMidlertidigForbudMotTiltakV2,
                    new List<string>()
                    {
                        FiksPlanMeldingtypeV2.ResultatMottat
                    }
                },
                {
                    FiksPlanMeldingtypeV2.FinnPlandokumenter,
                    new List<string>()
                    {
                        FiksPlanMeldingtypeV2.ResultatFinnPlandokumenter
                    }
                },
                {
                    FiksPlanMeldingtypeV2.FinnPlanbehandlinger,
                    new List<string>()
                    {
                        FiksPlanMeldingtypeV2.ResultatFinnPlanbehandlinger
                    }
                },
                {
                    FiksPlanMeldingtypeV2.FinnPlanerForOmraade,
                    new List<string>()
                    {
                        FiksPlanMeldingtypeV2.ResultatFinnPlanerForOmraade
                    }
                },
                {
                    FiksPlanMeldingtypeV2.HentArealplan,
                    new List<string>()
                    {
                        FiksPlanMeldingtypeV2.ResultatHentArealplan
                    }
                },
                {
                    FiksPlanMeldingtypeV2.SjekkMidlertidigForbud,
                    new List<string>()
                    {
                        FiksPlanMeldingtypeV2.ResultatSjekkMidlertidigForbud
                    }
                },
                {
                    FiksPlanMeldingtypeV2.RegistrerMidlertidigForbudMotTiltak,
                    new List<string>()
                    {
                        FiksPlanMeldingtypeV2.ResultatMottat
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
