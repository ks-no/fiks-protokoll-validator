using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using KS.Fiks.Arkiv.Models.V1.Meldingstyper;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation.Resources;
using Newtonsoft.Json.Linq;
using Serilog;
using Wmhelp.XPath2;

namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation
{
    public class FiksResponseValidator : IFiksResponseValidator
    {
        private static HashSet<string> _messageTypesWithPayloads;
        private static readonly ILogger Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
        private static PayloadValidator _payloadValidator;

        public FiksResponseValidator()
        {
            _payloadValidator = new PayloadValidator();
        }

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

                if(fiksRequest.TestCase.MessageType.Equals(FiksArkivMeldingtype.Sok))
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

                    ValidatePayload(fiksResponse, fiksRequest, 
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

        private static void ValidatePayload(FiksResponse fiksResponse, FiksRequest fiksRequest,
            List<string> validationErrors)
        {
            var fiksPayload = GetFiksPayload(fiksResponse.FiksPayloads);
            var receivedPayloadFileName = fiksPayload != null ? fiksPayload.Filename : null;
            var messageType = fiksResponse.Type;

            // Ingen payload forventet og heller ingen payload mottatt. Trenger ikke videre validering. Alt ok.
            if (!ShouldHavePayload(messageType) && receivedPayloadFileName == null)
                return;
            
            // Forventet payload men ingen mottatt. Stopp videre validering. Feil!
            if (ShouldHavePayload(messageType) && receivedPayloadFileName == null)
            {
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.MissingPayloadFileMessage, messageType
                ));
                return;
            }

            // Ingen paylod forventet men det er mottatt fil. Stopp videre validering. Feil!
            if (!ShouldHavePayload(messageType) && receivedPayloadFileName != null) 
            {
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.UnexpectedPayloadFileMessage, messageType
                ));
                return;
            }

            // Skulle vært asice signed, men gå videre til resten av valideringen. Rapporter feil!
            if (ShouldHavePayload(messageType) && !fiksResponse.IsAsiceVerified)
            {
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.MissingAsiceSigning, fiksResponse.PayloadErrors
                ));
            }
            
            // Har en feil i payload. Rapporter feil!
            if (ShouldHavePayload(messageType) && !string.IsNullOrEmpty(fiksResponse.PayloadErrors))
            {
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.PayloadError, fiksResponse.PayloadErrors
                ));
            }

            // Payload mottat som forventet men feil filformat. Feil!
            if (receivedPayloadFileName != null && !_payloadValidator.HasValidFileFormat(receivedPayloadFileName))
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
                    ValidationErrorMessages.InvalidPayloadFilename, receivedPayloadFileName, PayloadValidator.GetExpectedFileName(messageType)
                ));
                return;
            }

            // Validering av innhold, xml og json 
            if (receivedPayloadFileName != null && receivedPayloadFileName.EndsWith(".xml"))
            {
                var xmlContent = Encoding.Default.GetString(fiksPayload.Payload);

                _payloadValidator.ValidateXmlWithSchema(xmlContent, validationErrors);
                ValidateXmlPayloadContent(xmlContent, fiksRequest, validationErrors);
            }
            else
            {
                if (receivedPayloadFileName != null && receivedPayloadFileName.EndsWith(".json"))
                {
                    _payloadValidator.ValidateJsonWithSchema(Encoding.Default.GetString(fiksPayload.Payload), validationErrors, messageType);
                    ValidateJsonPayloadContent(Encoding.Default.GetString(fiksPayload.Payload), fiksRequest.TestCase.FiksResponseTests, validationErrors);
                }
            }
        }

        private static FiksPayload GetFiksPayload(List<FiksPayload> fiksPayloads)
        {
            return fiksPayloads is { Count: > 0 } ? fiksPayloads[0] : null;
        }

        private static bool ShouldHavePayload(string responseMessageType)
        {
            _messageTypesWithPayloads ??= PayloadValidator.GetMessageTypesWithPayload();
            return _messageTypesWithPayloads.Contains(responseMessageType);
        }

        private static bool HasCorrectFilename(string messageType, string filename)
        {
            return PayloadValidator.GetExpectedFileName(messageType).Equals(filename);
        }

       public static void ValidateXmlPayloadContent(string xmlPayloadContent, FiksRequest fiksRequest,
            List<string> validationErrors)
        {
            // If Fiks-Arkiv search, check result against the actual search request
            //TODO either fix this search validation or delete it. Doesnt work at the moment
            /*
             if (fiksRequest.TestCase.MessageType == FiksArkivMeldingtype.Sok)
            {
                SokeresultatValidator.ValidateXmlPayloadWithSokRequest(xmlPayloadContent, fiksRequest, validationErrors);
            }
            */

            try
            {
                // Use of xpath checks in testinformation.json
                var xmlDoc = XDocument.Parse(xmlPayloadContent);

                if (fiksRequest.TestCase.FiksResponseTests != null)
                {
                    ValidateXmlPayloadWithFiksResponseTests(fiksRequest, validationErrors, xmlDoc);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Klarte ikke å parse xml for validering: {xmlPayloadContent}");
                validationErrors.Add("Klarte ikke å parse xml for validering.");
            }
        }

        private static void ValidateXmlPayloadWithFiksResponseTests(FiksRequest fiksRequest, List<string> validationErrors,
           XDocument xmlDoc)
        {
           foreach (var fiksResponseTest in fiksRequest.TestCase.FiksResponseTests)
           {
               var expectedElement = fiksResponseTest.PayloadQuery.Split('/').Last();
               var expectedValue = fiksResponseTest.ExpectedValue;
               var expectedValueType = fiksResponseTest.ValueType;

               var xpathQuery = fiksResponseTest.PayloadQuery.Replace("/", "/*:");

               XElement node = null;
               try
               {
                   node = xmlDoc.XPath2SelectElement(xpathQuery);
               }
               catch (Exception e)
               {
                   Log.Error("Klarte ikke validere mot xpathQuery {Xpathquery}", xpathQuery);
                   node = null;
               }

               if (node == null)
               {
                   validationErrors.Add(string.Format(
                       ValidationErrorMessages.MissingPayloadElement, fiksResponseTest.PayloadQuery
                   ));
                   continue;
               }

               if (expectedValueType == SearchValueType.Attribute)
               {
                   if (expectedValue.Contains("*"))
                   {
                       var strippedExpectedValue = expectedValue.Replace("*", "");
                       if (!node.Attributes().Any(a => a.Value.Contains(strippedExpectedValue)))
                       {
                           validationErrors.Add(string.Format(
                               ValidationErrorMessages.MissingAttributeOnPayloadElement, expectedValue, expectedElement
                           ));
                       }
                   }
                   else if (!node.Attributes().Any(a => a.Value.Equals(expectedValue)))
                   {
                       validationErrors.Add(string.Format(
                           ValidationErrorMessages.MissingAttributeOnPayloadElement, expectedValue, expectedElement
                       ));
                   }
               }
               else if (expectedValueType == SearchValueType.ValueEqual)
               {
                   if (string.IsNullOrWhiteSpace(node.Value))
                       validationErrors.Add(string.Format(
                           ValidationErrorMessages.MissingValueOnPayloadElement, expectedElement
                       ));
                   else if (expectedValue == "*")
                       continue;
                   else if (expectedValue != null && node.Value != expectedValue)
                       validationErrors.Add(string.Format(
                           ValidationErrorMessages.WrongValueOnPayloadElement, expectedElement, expectedValue,
                           node.Value
                       ));
               }
               else if (expectedValueType == SearchValueType.YearNow)
               {
                   if (node.Value != DateTime.Now.Year.ToString())
                   {
                       validationErrors.Add(string.Format(
                           ValidationErrorMessages.WrongValueOnPayloadElement, expectedElement, expectedValue,
                           node.Value
                       ));
                   }
               }
               else if (expectedValueType == SearchValueType.Regex)
               {
                   var m = Regex.Match(node.Value, expectedValue);
                   if (!m.Success)
                   {
                       validationErrors.Add(string.Format(
                           ValidationErrorMessages.FailedRegexPattern, expectedElement, expectedValue,
                           node.Value
                       ));
                   }
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
                    else if (expectedValueType == SearchValueType.ValueEqual)
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
