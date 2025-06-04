using System;
using System.Collections.Generic;
using KS.Fiks.Arkiv.Models.V1.Meldingstyper;
using KS.Fiks.Plan.Models.V2.Meldingstyper;
using KS.Fiks.Saksfaser.Models.V1.Meldingstyper;
using KS.FiksProtokollValidator.WebAPI.Resources;

namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation
{
    public class PayloadValidator : IDisposable
    {
        private readonly JsonValidator _jsonValidatorForSaksfaser = JsonValidator.Init().WithFiksSaksfaser();
        private readonly JsonValidator _jsonValidatorForPlan = JsonValidator.Init().WithFiksPlan();

        public static HashSet<string> GetMessageTypesWithPayload()
        {
            return new HashSet<string>
            {
                FiksArkivMeldingtype.ArkivmeldingOpprettKvittering,
                FiksArkivMeldingtype.SokResultatMinimum,
                FiksArkivMeldingtype.SokResultatNoekler,
                FiksArkivMeldingtype.SokResultatUtvidet,
                FiksArkivMeldingtype.RegistreringHentResultat,
                FiksArkivMeldingtype.MappeHentResultat,
                FiksArkivMeldingtype.DokumentfilHentResultat,
                FiksArkivMeldingtype.DokumentobjektOpprettKvittering,
                FiksArkivMeldingtype.Ugyldigforespørsel,
                FiksArkivMeldingtype.Ikkefunnet,
                FiksArkivMeldingtype.Serverfeil,
                ResponseMessageTypes.FeilV1, //TODO er denne i bruk?
                FiksPlanMeldingtypeV2.ResultatFinnArealplaner,
                FiksPlanMeldingtypeV2.ResultatFinnDispensasjoner,
                FiksPlanMeldingtypeV2.ResultatFinnPlanbehandlinger,
                FiksPlanMeldingtypeV2.KvitteringOpprettArealplan,
                FiksPlanMeldingtypeV2.ResultatHentAktoerer,
                FiksPlanMeldingtypeV2.ResultatHentArealplan,
                FiksPlanMeldingtypeV2.ResultatHentRelatertePlaner,
                FiksPlanMeldingtypeV2.ResultatHentGjeldendePlandokumenter,
                FiksPlanMeldingtypeV2.ResultatHentKodeliste,
                FiksPlanMeldingtypeV2.OpprettArealplan,
                FiksPlanMeldingtypeV2.ResultatHentPlanomraader,
                FiksPlanMeldingtypeV2.ResultatFinnMidlertidigForbud,
                FiksSaksfaserMeldingtyperV1.HentSaksfase,
                FiksSaksfaserMeldingtyperV1.HentSaksfaser,
                FiksSaksfaserMeldingtyperV1.ResultatHentSaksfaser,
                FiksSaksfaserMeldingtyperV1.ResultatHentSaksfaser
            };
            //NB Husk at man må fylle på i denne listen med de meldingstyper som har resultat.
        }

        public static string GetExpectedFileName(string messageType)
        {
            switch (messageType)
            {
                case FiksArkivMeldingtype.ArkivmeldingOpprett:
                case FiksArkivMeldingtype.ArkivmeldingOppdater:
                case FiksArkivMeldingtype.AvskrivningOpprett:
                case FiksArkivMeldingtype.AvskrivningSlett:
                case FiksArkivMeldingtype.DokumentfilHent:
                case FiksArkivMeldingtype.MappeHent:
                case FiksArkivMeldingtype.RegistreringHent:
                    return "arkivmelding.xml";
                case FiksArkivMeldingtype.ArkivmeldingOpprettKvittering:
                    return "arkivmelding-kvittering.xml";
                case FiksArkivMeldingtype.Sok:
                    return "sok.xml";
                case FiksArkivMeldingtype.SokResultatMinimum:
                case FiksArkivMeldingtype.SokResultatNoekler:
                case FiksArkivMeldingtype.SokResultatUtvidet:
                case FiksArkivMeldingtype.RegistreringHentResultat:
                case FiksArkivMeldingtype.DokumentfilHentResultat:
                case FiksArkivMeldingtype.MappeHentResultat:
                    return "resultat.xml";
                case FiksArkivMeldingtype.Ugyldigforespørsel:
                case FiksArkivMeldingtype.Serverfeil:
                case FiksArkivMeldingtype.Ikkefunnet:
                    return "feilmelding.xml";
                case FiksPlanMeldingtypeV2.ResultatFinnDispensasjoner:
                case FiksPlanMeldingtypeV2.ResultatFinnPlanbehandlinger:
                case FiksPlanMeldingtypeV2.ResultatFinnArealplaner:
                case FiksPlanMeldingtypeV2.ResultatHentAktoerer:
                case FiksPlanMeldingtypeV2.ResultatHentArealplan:
                case FiksPlanMeldingtypeV2.ResultatHentGjeldendePlandokumenter:
                case FiksPlanMeldingtypeV2.ResultatHentKodeliste:
                case FiksPlanMeldingtypeV2.ResultatHentPlanomraader:
                case FiksPlanMeldingtypeV2.ResultatHentRelatertePlaner:
                case FiksPlanMeldingtypeV2.KvitteringOpprettArealplan:
                case FiksPlanMeldingtypeV2.ResultatFinnMidlertidigForbud:
                case FiksSaksfaserMeldingtyperV1.HentSaksfaser:
                case FiksSaksfaserMeldingtyperV1.HentSaksfase:
                case FiksSaksfaserMeldingtyperV1.ResultatHentSaksfaser:
                case FiksSaksfaserMeldingtyperV1.ResultatHentSaksfase:
                    return "payload.json";
                default:
                    return string.Empty;
            }
        }

        public bool HasValidFileFormat(string receivedPayloadFileName)
        {
            return receivedPayloadFileName.EndsWith(".xml") ||
                   receivedPayloadFileName.EndsWith(".json") ||
                   receivedPayloadFileName.EndsWith(".txt");
        }

        public void ValidateJsonWithSchema(string payload, List<string> validationErrors, string messageType)
        {
            if (messageType.StartsWith("no.ks.fiks.saksfaser"))
            {
                _jsonValidatorForSaksfaser.Validate(payload, validationErrors, messageType);
            } else if (messageType.StartsWith("no.ks.fiks.plan"))
            {
                _jsonValidatorForPlan.Validate(payload, validationErrors, messageType);
            }
        }

        internal void ValidateXmlWithSchema(string xmlPayloadContent, List<string> validationErrors)
        {
            var xsdValidator = new XsdValidator();
            xsdValidator.Validate(xmlPayloadContent, validationErrors);
        }

        public void Dispose()
        {
            _jsonValidatorForSaksfaser.Dispose();
            _jsonValidatorForPlan.Dispose();
        }
    }
}