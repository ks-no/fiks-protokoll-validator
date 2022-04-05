using System.Collections.Generic;
using System.IO;
using System.Reflection;
using KS.Fiks.Arkiv.Models.V1.Meldingstyper;
using KS.Fiks.IO.Client.Models.Feilmelding;
using KS.Fiks.IO.Politiskbehandling.Client.Models;
using KS.Fiks.Plan.Client.Models;
using KS.FiksProtokollValidator.WebAPI.Resources;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace KS.FiksProtokollValidator.WebAPI.Validation
{
    public static class PayloadChecksHelper
    {
        public static HashSet<string> GetMessageTypesWithPayload()
        { //NB Husk at man må fylle på i denne listen med de meldingstyper som har resultat.
            return new HashSet<string>
            {
                FiksArkivV1Meldingtype.ArkivmeldingKvittering,
                FiksArkivV1Meldingtype.SokResultatMinimum,
                FiksArkivV1Meldingtype.SokResultatNoekler,
                FiksArkivV1Meldingtype.SokResultatUtvidet,
                ResponseMessageTypes.FeilV1, //TODO er denne i bruk?
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
                case FiksArkivV1Meldingtype.Arkivmelding:
                    return "arkivmelding.xml";
                case FiksArkivV1Meldingtype.ArkivmeldingKvittering:
                    return "arkivmelding-kvittering.xml";
                case FiksArkivV1Meldingtype.Sok:
                    return "sok.xml";
                case FiksArkivV1Meldingtype.SokResultatMinimum:
                    return "sokeresultat-minimum.xml";
                case FiksArkivV1Meldingtype.SokResultatNoekler:
                    return "sokeresultat-noekler.xml";
                case FiksArkivV1Meldingtype.SokResultatUtvidet:
                    return "sokeresultat-utvidet.xml";
                case FiksArkivV1Meldingtype.DokumentfilHent:
                case FiksArkivV1Meldingtype.DokumentfilHentResultat:
                case FiksArkivV1Meldingtype.MappeHent:
                case FiksArkivV1Meldingtype.MappeHentResultat:
                case FiksArkivV1Meldingtype.JournalpostHent:
                case FiksArkivV1Meldingtype.JournalpostHentResultat:
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

        internal static void ValidateJsonWithSchema(string payload, List<string> validationErrors, string messageType)
        {
            var baseDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            var pathToSchema = Path.Combine(baseDirectory, "Schema", messageType + ".schema.json");
            using (TextReader file = File.OpenText(pathToSchema))
            {
                JObject jObject = JObject.Parse(payload);
                JSchema schema = JSchema.Parse(file.ReadToEnd());

                schema.ExtensionData.Remove("definitions");
                AddAdditionalPropertiesFalseToSchemaProperties(schema.Properties);
                schema.AllowAdditionalProperties = false;

                //TODO:Skille mellom errors og warnings hvis det er 
                jObject.Validate(schema, (o, a) =>
                {
                    validationErrors.Add(a.Message);
                });
            }
        }

        private static void AddAdditionalPropertiesFalseToSchemaProperties(IDictionary<string, JSchema> properties)
        {
            foreach (var item in properties)
            {
                item.Value.AllowAdditionalProperties = false;
                foreach (var itemItem in item.Value.Items)
                {
                    AddAdditionalPropertiesFalseToSchemaProperties(itemItem.Properties);

                }
                AddAdditionalPropertiesFalseToSchemaProperties(item.Value.Properties);
            }
        }

        internal static void ValidateXmlWithSchema(string xmlPayloadContent, List<string> validationErrors, string messageType)
        {
            var xsdValidator = new XsdValidator();
            switch (messageType)
            {
                case FiksArkivV1Meldingtype.ArkivmeldingKvittering:
                    xsdValidator.ValidateArkivmeldingKvittering(xmlPayloadContent, validationErrors);
                    break;
                case FiksArkivV1Meldingtype.SokResultatMinimum:
                    xsdValidator.ValidateArkivmeldingSokeresultatMinimum(xmlPayloadContent, validationErrors);
                    break;
                case FiksArkivV1Meldingtype.SokResultatNoekler:
                    xsdValidator.ValidateArkivmeldingSokeresultatNoekler(xmlPayloadContent, validationErrors);
                    break;
                case FiksArkivV1Meldingtype.SokResultatUtvidet:
                    xsdValidator.ValidateArkivmeldingSokeresultatUtvidet(xmlPayloadContent, validationErrors);
                    break;
                default:
                    //do nothing? Or display a warning that the message type was not checked against xsd?
                    break;
            }
        }
    }
}