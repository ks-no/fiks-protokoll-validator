using System.Collections.Generic;
using KS.Fiks.Arkiv.Models.V1.Meldingstyper;
using KS.Fiks.IO.Politiskbehandling.Client.Models;
using KS.Fiks.Plan.Models.V2.Meldingstyper;
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
                        FiksPlanMeldingtypeV2.FinnArealplanerForMatrikkelenhet,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatFinnArealplaner
                    }
                },
                {
                    FiksPlanMeldingtypeV2.RegistrerDispensasjon,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.KvitteringRegistrerDispensasjon
                    }
                },
                {
                    FiksPlanMeldingtypeV2.RegistrerPlanavgrensning,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.KvitteringRegistrerPlanavgrensning
                    }
                },
                {
                    FiksPlanMeldingtypeV2.FinnArealplaner,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatFinnArealplaner
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
                        FiksPlanMeldingtypeV2.KvitteringOpprettArealplan
                    }
                },
                {
                    FiksPlanMeldingtypeV2.RegistrerPlanbehandling,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.KvitteringRegistrerPlanbehandling
                    }
                },
                {
                    FiksPlanMeldingtypeV2.OppdaterArealplan,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.KvitteringOppdaterArealplan
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
                    FiksPlanMeldingtypeV2.HentKodeliste,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatHentKodeliste
                    }
                },
                {
                    FiksPlanMeldingtypeV2.HentDokumentfil,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatHentDokumentfil
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
                    FiksPlanMeldingtypeV2.HentGjeldendePlandokumenter,
                    new List<string>
                    {
                        FiksPlanMeldingtypeV2.ResultatHentGjeldendePlandokumenter
                    }
                },
                {
                    FiksPlanMeldingtypeV2.OppdaterDispensasjon,
                    new List<string>()
                    {
                        FiksPlanMeldingtypeV2.KvitteringOppdaterDispensasjon
                    }
                },
                {
                    RequestMessageTypes.RegistrerMidlertidigForbudMotTiltakV2,
                    new List<string>()
                    {
                        FiksPlanMeldingtypeV2.KvitteringRegistrerMidlertidigForbudMotTiltak
                    }
                },
                {
                    FiksPlanMeldingtypeV2.FinnArealplaner,
                    new List<string>()
                    {
                        FiksPlanMeldingtypeV2.ResultatFinnArealplaner
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
                    FiksPlanMeldingtypeV2.FinnArealplanerForFlate,
                    new List<string>()
                    {
                        FiksPlanMeldingtypeV2.ResultatFinnArealplaner
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
                    FiksPlanMeldingtypeV2.FinnMidlertidigForbudMotTiltak,
                    new List<string>()
                    {
                        FiksPlanMeldingtypeV2.ResultatFinnMidlertidigForbud
                    }
                },
                {
                    FiksPlanMeldingtypeV2.RegistrerMidlertidigForbudMotTiltak,
                    new List<string>()
                    {
                        FiksPlanMeldingtypeV2.KvitteringRegistrerMidlertidigForbudMotTiltak
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
