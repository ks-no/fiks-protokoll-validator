using System.Collections.Generic;
using System.IO;
using System.Reflection;
using KS.Fiks.Arkiv.Models.V1.Meldingstyper;
using KS.Fiks.IO.Politiskbehandling.Client.Models;
using KS.Fiks.Plan.Client.Models;
using KS.Fiks.Protokoller.V1.Models.Feilmelding;
using KS.FiksProtokollValidator.WebAPI.Resources;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace KS.FiksProtokollValidator.WebAPI.Validation
{
    public static class PayloadChecksHelper
    {
        public static HashSet<string> GetMessageTypesWithPayload()
        {
            return new HashSet<string>
            {
                FiksArkivMeldingtype.ArkivmeldingKvittering,
                FiksArkivMeldingtype.SokResultatMinimum,
                FiksArkivMeldingtype.SokResultatNoekler,
                FiksArkivMeldingtype.SokResultatUtvidet,
                FiksArkivMeldingtype.JournalpostHentResultat,
                FiksArkivMeldingtype.MappeHentResultat,
                FiksArkivMeldingtype.DokumentfilHentResultat,
                FiksArkivMeldingtype.Ugyldigforespørsel,
                FiksArkivMeldingtype.Ikkefunnet,
                FiksArkivMeldingtype.Serverfeil,
                ResponseMessageTypes.FeilV1, //TODO er denne i bruk?
                PolitiskBehandlingMeldingTypeV1.ResultatMoeteplan,
                PolitiskBehandlingMeldingTypeV1.ResultatUtvalg,
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
            //NB Husk at man må fylle på i denne listen med de meldingstyper som har resultat.
        }

        public static string GetExpectedFileName(string messageType)
        {
            switch (messageType)
            {
                case FiksArkivMeldingtype.Arkivmelding:
                case FiksArkivMeldingtype.DokumentfilHent:
                case FiksArkivMeldingtype.MappeHent:
                case FiksArkivMeldingtype.JournalpostHent:
                    return "arkivmelding.xml";
                case FiksArkivMeldingtype.ArkivmeldingKvittering:
                    return "arkivmelding-kvittering.xml";
                case FiksArkivMeldingtype.Sok:
                    return "sok.xml";
                case FiksArkivMeldingtype.SokResultatMinimum:
                case FiksArkivMeldingtype.SokResultatNoekler:
                case FiksArkivMeldingtype.SokResultatUtvidet:
                case FiksArkivMeldingtype.JournalpostHentResultat:
                case FiksArkivMeldingtype.DokumentfilHentResultat:
                case FiksArkivMeldingtype.MappeHentResultat:
                    return "resultat.xml";
                case FiksArkivMeldingtype.Ugyldigforespørsel:
                case FiksArkivMeldingtype.Serverfeil:
                case FiksArkivMeldingtype.Ikkefunnet:
                    return "feilmelding.xml";
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
            switch (messageType)
            {
                case FeilmeldingType.Ikkefunnet:
                case FeilmeldingType.Serverfeil:
                case FeilmeldingType.Ugyldigforespørsel:
                    var fiksProtokollerAssembly = Assembly.Load("KS.Fiks.Protokoller.V1");
                    using (var schemaStream =
                        fiksProtokollerAssembly.GetManifestResourceStream($"KS.Fiks.Protokoller.V1.Schema.{messageType}.schema.json"))
                    {
                        if (schemaStream != null)
                        {
                            var reader = new StreamReader(schemaStream);
                            var schemaString = reader.ReadToEnd();
                            ValidateJsonWithSchemaString(payload, validationErrors, schemaString);
                        }
                    }
                    break;
                default:
                    var baseDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
                    var pathToSchema = Path.Combine(baseDirectory, "Schema", messageType + ".schema.json");
                    using (TextReader file = File.OpenText(pathToSchema))
                    {
                        var schemaString = file.ReadToEnd();
                        ValidateJsonWithSchemaString(payload, validationErrors, schemaString);
                    }
                    break;
            }
        }

        private static void ValidateJsonWithSchemaString(string payload, List<string> validationErrors, string schemaString)
        {
            var jObject = JObject.Parse(payload);
            var schema = JSchema.Parse(schemaString);

            schema.ExtensionData.Remove("definitions");
            AddAdditionalPropertiesFalseToSchemaProperties(schema.Properties);
            schema.AllowAdditionalProperties = false;

            //TODO:Skille mellom errors og warnings hvis det er 
            jObject.Validate(schema, (o, a) => { validationErrors.Add(a.Message); });
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

        internal static void ValidateXmlWithSchema(string xmlPayloadContent, List<string> validationErrors)
        {
            var xsdValidator = new XsdValidator();
            xsdValidator.Validate(xmlPayloadContent, validationErrors);
        }
    }
}