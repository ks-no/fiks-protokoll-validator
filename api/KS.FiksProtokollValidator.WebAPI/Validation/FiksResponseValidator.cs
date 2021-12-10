﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using KS.Fiks.IO.Arkiv.Client.Models;
using KS.Fiks.IO.Client.Models.Feilmelding;
using KS.Fiks.IO.Politiskbehandling.Client.Models;
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
            var found = false;
            var foundExpectedResponseMessageTypes = new List<string>();
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
            FiksPayload fiksPayload = GetFiksPayload(fiksResponse.FiksPayloads);
            var receivedPayloadFileName = fiksPayload != null ? fiksPayload.Filename : null;
            var messageType = fiksResponse.Type;

            if (!ResponseMessageShouldHavePayload(messageType) && receivedPayloadFileName == null)
                return;

            if (ResponseMessageShouldHavePayload(messageType) && receivedPayloadFileName == null)
            {
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.MissingPayloadFileMessage, messageType
                ));
                return;
            }

            if (receivedPayloadFileName != null && !ResponseMessageShouldHavePayload(messageType))
            {
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.UnexpectedPayloadFileMessage, messageType
                ));
                return;
            }

            if (receivedPayloadFileName != null && !hasAllowedFileFormat(receivedPayloadFileName))
            {
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.InvalidPayloadFileFormatMessage, receivedPayloadFileName.Split('.').Last()
                ));
                return;
            }

            if (receivedPayloadFileName.EndsWith(".xml"))
            {
                ValidateXmlPayloadContent(System.Text.Encoding.Default.GetString(fiksPayload.Payload), fiksResponseTests, validationErrors);
            }
            else
            {
                if (receivedPayloadFileName.EndsWith(".json"))
                {
                    ValidateJsonPayloadContent(System.Text.Encoding.Default.GetString(fiksPayload.Payload), fiksResponseTests, validationErrors);
                }
            }
        }

        private static FiksPayload GetFiksPayload(List<FiksPayload> fiksPayloads)
        {
            return fiksPayloads != null && fiksPayloads.Count > 0 ? fiksPayloads[0] : null;
        }

        private static bool hasAllowedFileFormat(string receivedPayloadFileName)
        {
            return receivedPayloadFileName.EndsWith(".xml") || 
                receivedPayloadFileName.EndsWith(".json") || 
                receivedPayloadFileName.EndsWith(".txt");
        }

        private static bool ResponseMessageShouldHavePayload(string responseMessageType)
        {
            _messageTypesWithPayloads ??= GetMessageTypesWithPayload();
            return _messageTypesWithPayloads.Contains(responseMessageType);
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
                WebAPI.Resources.ResponseMessageTypes.FinnPlanerForMatrikkelenhetV2,
                WebAPI.Resources.ResponseMessageTypes.FinnPlanerV2,
                WebAPI.Resources.ResponseMessageTypes.FinnDispensasjonerForSøkV2,
                WebAPI.Resources.ResponseMessageTypes.MeldingOmPlanidentV2,
                WebAPI.Resources.ResponseMessageTypes.AktørerV2,
                WebAPI.Resources.ResponseMessageTypes.BboxV2,
                WebAPI.Resources.ResponseMessageTypes.RelatertePlanerV2,
                WebAPI.Resources.ResponseMessageTypes.GjeldendePlanbestemmelserV2,
                WebAPI.Resources.ResponseMessageTypes.KodelisteV2,
                WebAPI.Resources.ResponseMessageTypes.PlandokumenterV2
            };
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
