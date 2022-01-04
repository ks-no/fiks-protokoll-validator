using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using KS.Fiks.IO.Arkiv.Client.Models;
using KS.Fiks.IO.Client.Models.Feilmelding;
using KS.Fiks.IO.Politiskbehandling.Client.Models;
using KS.Fiks.Plan.Client.Models;
using KS.FiksProtokollValidator.WebAPI.Models;
using KS.FiksProtokollValidator.WebAPI.Validation.Resources;
using Newtonsoft.Json.Linq;
using Wmhelp.XPath2;

namespace KS.FiksProtokollValidator.WebAPI.Validation
{
    public class FiksResponseValidator : IFiksResponseValidator
    {
        private static HashSet<string> _messageTypesWithPayloads;

        public void Validate(TestSession testSession)
        {
            foreach (var fiksRequest in testSession.FiksRequests)
            {
                if (!fiksRequest.FiksResponses.Any())
                    continue;

                var expectedResponseMessageTypes =
                    ExpectedResponseMessageTypeProvider.GetExpectedResponseMessageTypesAsStrings(
                        fiksRequest.TestCase.MessageType, fiksRequest.TestCase.ExpectedResponseMessageTypes
                    );

                fiksRequest.FiksResponseValidationErrors = new List<string>();

                if(fiksRequest.TestCase.MessageType.Equals(ArkivintegrasjonMeldingTypeV1.Sok))
                {
                    ValidateExistenceOfOneOfExpectedResponseMessageTypes(fiksRequest, expectedResponseMessageTypes);
                }
                else
                {
                    ValidateExistenceOfExpectedResponseMessageTypes(fiksRequest, expectedResponseMessageTypes);
                }

                foreach (var fiksResponse in fiksRequest.FiksResponses)
                {
                    ValidateActualResponseMessageType(fiksResponse.Type, expectedResponseMessageTypes,
                        fiksRequest.FiksResponseValidationErrors);

                    ValidatePayload(fiksResponse, fiksRequest.TestCase.FiksResponseTests,
                        fiksRequest.FiksResponseValidationErrors);
                }

                fiksRequest.IsFiksResponseValidated = true;
            }
        }

        private static void ValidateExistenceOfExpectedResponseMessageTypes(FiksRequest fiksRequest,
            IEnumerable<string> expectedResponseMessageTypes)
        {
            foreach (var expectedResponseMessageType in expectedResponseMessageTypes)
            {
                if (!fiksRequest.FiksResponses.Any(r => r.Type.Equals(expectedResponseMessageType)))
                {
                    var validationError = string.Format(
                        ValidationErrorMessages.MissingResponseMessage, expectedResponseMessageType
                    );

                    fiksRequest.FiksResponseValidationErrors.Add(validationError);
                }
            }
        }
        
        private static void ValidateExistenceOfOneOfExpectedResponseMessageTypes(FiksRequest fiksRequest,
            IEnumerable<string> expectedResponseMessageTypes)
        {
            foreach (var expectedResponseMessageType in expectedResponseMessageTypes)
            {
                if (fiksRequest.FiksResponses.Any(r => r.Type.Equals(expectedResponseMessageType)))
                {
                    return;
                }
            }

            foreach (var expectedResponseMessageType in expectedResponseMessageTypes)
            {
                var validationError = string.Format(
                    ValidationErrorMessages.MissingResponseMessage, expectedResponseMessageType
                );
                fiksRequest.FiksResponseValidationErrors.Add(validationError);
            }
        }

        private static void ValidateActualResponseMessageType(string responseMessageType,
            IEnumerable<string> expectedResponseMessageTypes, List<string> validationErrors)
        {
            if (!expectedResponseMessageTypes.Contains(responseMessageType))
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.UnexpectedResponseMessage, responseMessageType
                ));
        }

        private static void ValidatePayload(FiksResponse fiksResponse, List<FiksResponseTest> fiksResponseTests,
            List<string> validationErrors)
        {
            var fiksPayload = GetFiksPayload(fiksResponse.FiksPayloads);
            var receivedPayloadFileName = fiksPayload != null ? fiksPayload.Filename : null;
            var messageType = fiksResponse.Type;

            // Ingen payload forventet og heller ingen payload mottatt. Alt ok.
            if (!ShouldHavePayload(messageType) && receivedPayloadFileName == null)
                return;

            // Forventet payload men ingen mottatt. Feil!
            if (ShouldHavePayload(messageType) && receivedPayloadFileName == null)
            {
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.MissingPayloadFileMessage, messageType
                ));
                return;
            }

            // Ingen paylod forventet men det er mottatt fil. Feil!
            if (!ShouldHavePayload(messageType) && receivedPayloadFileName != null) 
            {
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.UnexpectedPayloadFileMessage, messageType
                ));
                return;
            }

            // Payload mottat som forventet men feil filformat. Feil!
            if (receivedPayloadFileName != null && !HasValidFileFormat(receivedPayloadFileName))
            {
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.InvalidPayloadFileFormatMessage, receivedPayloadFileName.Split('.').Last()
                ));
                return;
            }

            // Payload har ikke korrekt filnavn i forhold til meldingstype. Feil!
            if (!HasCorrectFilename(fiksResponse.Type, receivedPayloadFileName))
            {
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.InvalidPayloadFilename, receivedPayloadFileName
                ));
                return;
            }

            if (receivedPayloadFileName != null && receivedPayloadFileName.EndsWith(".xml"))
            {
                var xmlContent = System.Text.Encoding.Default.GetString(fiksPayload.Payload);
                ValidateXmlWithSchema(xmlContent, validationErrors);
                ValidateXmlPayloadContent(xmlContent, fiksResponseTests, validationErrors);
            }
            else
            {
                if (receivedPayloadFileName != null && receivedPayloadFileName.EndsWith(".json"))
                {
                    ValidateJsonPayloadContent(System.Text.Encoding.Default.GetString(fiksPayload.Payload), fiksResponseTests, validationErrors);
                }
            }
        }

        private static FiksPayload GetFiksPayload(List<FiksPayload> fiksPayloads)
        {
            return fiksPayloads != null && fiksPayloads.Count > 0 ? fiksPayloads[0] : null;
        }

        private static bool HasValidFileFormat(string receivedPayloadFileName)
        {
            return receivedPayloadFileName.EndsWith(".xml") || 
                receivedPayloadFileName.EndsWith(".json") || 
                receivedPayloadFileName.EndsWith(".txt");
        }

        private static bool ShouldHavePayload(string responseMessageType)
        {
            _messageTypesWithPayloads ??= GetMessageTypesWithPayload();
            return _messageTypesWithPayloads.Contains(responseMessageType);
        }

        private static bool HasCorrectFilename(string messageType, string filename)
        {
            return GetExpectedFileName(messageType).Equals(filename);
        }
        
        
        private static string GetExpectedFileName(string messageType)
        {
            switch (messageType)
            {
                case ArkivintegrasjonMeldingTypeV1.Arkivmelding:
                    return "arkivmelding.xml";
                case ArkivintegrasjonMeldingTypeV1.ArkivmeldingKvittering:
                    return "arkivmeldingKvittering.xml";
                case ArkivintegrasjonMeldingTypeV1.Sok:
                    return "sok.xml";
                case ArkivintegrasjonMeldingTypeV1.SokResultatMinimum:
                    return "sokeresultatMinimum.xml";
                case ArkivintegrasjonMeldingTypeV1.SokResultatNoekler:
                    return "sokeresultatNoekler.xml";
                case ArkivintegrasjonMeldingTypeV1.SokResultatUtvidet:
                    return "sokeresultatUtvidet.xml";
                case PolitiskBehandlingMeldingTypeV1.HentMoeteplan:
                case PolitiskBehandlingMeldingTypeV1.HentUtvalg:
                case PolitiskBehandlingMeldingTypeV1.SendOrienteringssak:
                case PolitiskBehandlingMeldingTypeV1.SendUtvalgssak:
                case PolitiskBehandlingMeldingTypeV1.SendDelegertVedtak:
                case PolitiskBehandlingMeldingTypeV1.SendVedtakFraUtvalg:
                    return "payload.json";
                default:
                    return string.Empty;
            }
        }

        private static HashSet<string> GetMessageTypesWithPayload()
        { //NB Husk at man må fylle på i denne listen med de meldingstyper som har resultat.
            return new()
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
                FiksPlanMeldingtypeV2.ResultatOpprettArealplan,
                FiksPlanMeldingtypeV2.ResultatHentAktoerer,
                FiksPlanMeldingtypeV2.ResultatHentBboxForPlan,
                FiksPlanMeldingtypeV2.ResultatHentRelatertePlaner,
                FiksPlanMeldingtypeV2.ResultatHentGjeldendePlanbestemmelser,
                FiksPlanMeldingtypeV2.ResultatHentKodeliste,
                FiksPlanMeldingtypeV2.ResultatFinnPlandokumenter
            };
        }

        private static void ValidateXmlWithSchema(string xmlPayloadContent, List<string> validationErrors)
        {
            XsdValidator.ValidateArkivmeldingKvittering(xmlPayloadContent, validationErrors);
        }

        private static void ValidateXmlPayloadContent(string xmlPayloadContent, List<FiksResponseTest> fiksResponseTests,
            List<string> validationErrors)
        {
            var xmlDoc = XDocument.Parse(xmlPayloadContent);

            foreach (var fiksResponseTest in fiksResponseTests)
            {
                var expectedElement = fiksResponseTest.PayloadQuery.Split('/').Last();
                var expectedValue = fiksResponseTest.ExpectedValue;
                var expectedValueType = fiksResponseTest.ValueType;

                var xpathQuery = fiksResponseTest.PayloadQuery.Replace("/", "/*:");
                var node = xmlDoc.XPath2SelectElement(xpathQuery);

                if (node == null)
                {
                    validationErrors.Add(string.Format(
                        ValidationErrorMessages.MissingPayloadElement, fiksResponseTest.PayloadQuery
                    ));
                    continue;
                }

                if (expectedValueType == SearchValueType.Attribute)
                {
                    if (!node.Attributes().Any(a => a.Value.Equals(expectedValue)))
                        validationErrors.Add(string.Format(
                            ValidationErrorMessages.MissingAttributeOnPayloadElement, expectedValue, expectedElement
                        ));
                }
                else if (expectedValueType == SearchValueType.Value)
                {
                    if (string.IsNullOrWhiteSpace(node.Value))
                        validationErrors.Add(string.Format(
                            ValidationErrorMessages.MissingValueOnPayloadElement, expectedElement
                        ));
                    else if (expectedValue == "*")
                        continue;
                    else if (expectedValue != null && node.Value != expectedValue)
                        validationErrors.Add(string.Format(
                            ValidationErrorMessages.WrongValueOnPayloadElement, expectedElement, expectedValue, node.Value
                        ));
                }
            }
        }
        private static void ValidateJsonPayloadContent(string jsonPayloadContent, List<FiksResponseTest> fiksResponseTests,
            List<string> validationErrors)
        {
            var json = JObject.Parse(jsonPayloadContent);

            if (json.Count == 0)
            {
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.MissingJsonPayload
                ));
            }
            else
            {

                foreach (var fiksResponseTest in fiksResponseTests)
                {
                    var path = fiksResponseTest.PayloadQuery;
                    var expectedValue = fiksResponseTest.ExpectedValue;
                    var expectedValueType = fiksResponseTest.ValueType;

                    if (expectedValueType == SearchValueType.Attribute)
                    {
                        var tokens = json.SelectTokens(path);
                       
                        if (tokens == null || tokens.Count() == 0)
                            validationErrors.Add(string.Format(
                                ValidationErrorMessages.MissingJsonPayloadToken, path
                            ));
                        else
                        {
                            bool foundExpectedValue = false;
                            for (int i = 0; i < tokens.Count(); i++)
                            {
                                JToken token = tokens.ElementAt(i);
                                var keyIsPresent = false;

                                if (token.Type == JTokenType.Array)
                                {
                                    JToken jTokenFromDirectPath = json.SelectToken(path+"["+i.ToString()+"]."+expectedValue);
                                    if (jTokenFromDirectPath != null)
                                    {
                                        keyIsPresent = true;
                                    }
                                }
                                else if (token.Type == JTokenType.Object)
                                {
                                    keyIsPresent = JObject.Parse(token.ToString()).ContainsKey(expectedValue);
                                }

                                    if (keyIsPresent)
                                    {
                                        foundExpectedValue = true;
                                    }
                                  
                            }
                            if (!foundExpectedValue)
                            {
                               validationErrors.Add(string.Format(ValidationErrorMessages.MissingAttributeOnPayloadElement, expectedValue, path));
                            }
                        }
                    }
                    else if (expectedValueType == SearchValueType.Value)
                    {
                        var tokens = json.SelectTokens(path);

                        if (!tokens.Any())
                            validationErrors.Add(string.Format(
                                ValidationErrorMessages.MissingJsonPayloadToken, path
                            ));

                        else if (expectedValue == "*")
                            //TODO: Sjekk at den ikke tom eller whitespace
                            continue;
                        
                        bool foundExpectedValue = false;
                        foreach (JToken token in tokens)
                        {
                            if (token.ToString() == expectedValue)
                            {
                                foundExpectedValue = true;
                            }
                        }
                        if (expectedValue != null && !foundExpectedValue)
                            validationErrors.Add(string.Format(
                                ValidationErrorMessages.WrongValueOnJsonPayloadKey, path, expectedValue
                            ));
                    }
                }
            }
        }
    }
}
