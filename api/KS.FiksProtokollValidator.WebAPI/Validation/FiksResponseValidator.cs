using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using KS.Fiks.IO.Arkiv.Client.Models;
using KS.Fiks.IO.Arkiv.Client.Models.Innsyn.Sok;
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

                    ValidatePayload(fiksResponse, fiksRequest);
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

        private static void ValidatePayload(FiksResponse fiksResponse, FiksRequest fiksRequest)
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
                fiksRequest.FiksResponseValidationErrors.Add(string.Format(
                    ValidationErrorMessages.MissingPayloadFileMessage, messageType
                ));
                return;
            }

            // Ingen paylod forventet men det er mottatt fil. Feil!
            if (!ShouldHavePayload(messageType) && receivedPayloadFileName != null) 
            {
                fiksRequest.FiksResponseValidationErrors.Add(string.Format(
                    ValidationErrorMessages.UnexpectedPayloadFileMessage, messageType
                ));
                return;
            }

            // Payload mottat som forventet men feil filformat. Feil!
            if (receivedPayloadFileName != null && !PayloadChecksHelper.HasValidFileFormat(receivedPayloadFileName))
            {
                fiksRequest.FiksResponseValidationErrors.Add(string.Format(
                    ValidationErrorMessages.InvalidPayloadFileFormatMessage, receivedPayloadFileName.Split('.').Last()
                ));
                return;
            }

            // Payload har ikke korrekt filnavn i forhold til meldingstype. Feil!
            if (!HasCorrectFilename(fiksResponse.Type, receivedPayloadFileName))
            {
                fiksRequest.FiksResponseValidationErrors.Add(string.Format(
                    ValidationErrorMessages.InvalidPayloadFilename, receivedPayloadFileName
                ));
                return;
            }

            if (receivedPayloadFileName != null && receivedPayloadFileName.EndsWith(".xml"))
            {
                var xmlContent = System.Text.Encoding.Default.GetString(fiksPayload.Payload);
                ValidateXmlWithSchema(xmlContent, fiksRequest.FiksResponseValidationErrors);
                ValidateXmlPayloadContent(xmlContent, fiksRequest.TestCase, fiksRequest.FiksResponseValidationErrors);
            }
            else
            {
                if (receivedPayloadFileName != null && receivedPayloadFileName.EndsWith(".json"))
                {
                    ValidateJsonPayloadContent(System.Text.Encoding.Default.GetString(fiksPayload.Payload), fiksRequest.TestCase.FiksResponseTests, fiksRequest.FiksResponseValidationErrors);
                }
            }
        }

        private static FiksPayload GetFiksPayload(List<FiksPayload> fiksPayloads)
        {
            return fiksPayloads is { Count: > 0 } ? fiksPayloads[0] : null;
        }

        private static bool ShouldHavePayload(string responseMessageType)
        {
            _messageTypesWithPayloads ??= PayloadChecksHelper.GetMessageTypesWithPayload();
            return _messageTypesWithPayloads.Contains(responseMessageType);
        }

        private static bool HasCorrectFilename(string messageType, string filename)
        {
            return PayloadChecksHelper.GetExpectedFileName(messageType).Equals(filename);
        }
        
        private static void ValidateXmlWithSchema(string xmlPayloadContent, List<string> validationErrors)
        {
            XsdValidator.ValidateArkivmeldingKvittering(xmlPayloadContent, validationErrors);
        }

        private static void ValidateXmlPayloadContent(string xmlPayloadContent, FiksRequest fiksRequest,
            ICollection<string> validationErrors)
        {
            if (fiksRequest.TestCase.MessageType == ArkivintegrasjonMeldingTypeV1.Sok)
            {
                var sokXml = ""; //TODO load sok.xml from TestCase
                if (fiksRequest.CustomPayloadFile != null)
                {
                    sokXml = GetCustomRequestXml(fiksRequest);
                }
                else
                {
                    sokXml = GetStandardRequestXml(fiksRequest);
                }
                using( var sokTextReader = (TextReader) new StringReader(sokXml))
                {
                    //Parse the sok request
                    var sok = (Sok) new XmlSerializer(typeof(Sok)).Deserialize(sokTextReader);

                    SokeresultatMinimum sokResponse = null;
                    //Parse the sok response
                    using (var sokResponseTextReader = (TextReader)new StringReader(xmlPayloadContent))
                    {
                        switch (sok.ResponsType)
                        {
                            case ResponsType.Minimum:
                                sokResponse =
                                    (SokeresultatMinimum)new XmlSerializer(typeof(SokeresultatMinimum)).Deserialize(
                                        sokResponseTextReader);
                                
                                break;
                        }
                    }

                    foreach (var parameter in sok.Parameter)
                    {
                        switch (parameter.Operator)
                        {
                            case OperatorType.Equal:
                                // sjekk at resultat inneholder forventet verdi 
                                break;
                            case OperatorType.Between:
                                if (isDateSokeFelt(parameter.Felt))
                                {
                                    var listOfDates = getDateResults(sokResponse, parameter.Felt);
                                    
                                    foreach (var dateTimeValue in listOfDates)
                                    {
                                        ValidateBetweenDates(dateTimeValue, parameter.Parameterverdier.Datevalues[0], parameter.Parameterverdier.Datevalues[1], validationErrors);
                                    }
                                }
                                break;
                        }   
                    }

                }
            }
            
            // Use of xpath checks in testinformation.json
            var xmlDoc = XDocument.Parse(xmlPayloadContent);

            foreach (var fiksResponseTest in fiksRequest.TestCase.FiksResponseTests)
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
                            ValidationErrorMessages.WrongValueOnPayloadElement, expectedElement, expectedValue, node.Value
                        ));
                } else if (expectedValueType == SearchValueType.YearNow)
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
                    if(!m.Success)
                    {
                        validationErrors.Add(string.Format(
                            ValidationErrorMessages.FailedRegexPattern, expectedElement, expectedValue,
                            node.Value
                        ));
                    }
                }
            }
        }

        private static string GetStandardRequestXml(FiksRequest fiksRequest)
        {
            throw new NotImplementedException();
        }

        private static string GetCustomRequestXml(FiksRequest fiksRequest)
        {
            var stream = new StreamReader(new MemoryStream(fiksRequest.CustomPayloadFile.Payload));
            return stream.ReadToEnd();
        }

        private static List<DateTime> getDateResults(SokeresultatMinimum sokResponse, SokFelt parameterFelt)
        {
            switch (parameterFelt)
            {
                case SokFelt.SakPeriodSaksdato:
                    return sokResponse.ResultatListe.Select(r => r.Saksmappe.Saksdato).ToList();
                
                default:
                    return new List<DateTime>();
            }
        }

        private static void ValidateBetweenDates(DateTime value, DateTime dateFrom, DateTime dateTo,
            ICollection<string> validationErrors)
        {
            if (value < dateFrom || value > dateTo)
            {
                validationErrors.Add($"Date {value} out of range from {dateFrom} to {dateTo}");   
            }
        }

        private static bool isDateSokeFelt(SokFelt parameterFelt)
        {
            switch (parameterFelt)
            {
                case SokFelt.JournalpostPeriodDokumentetsdato:
                case SokFelt.JournalpostPeriodForfallsdato:
                case SokFelt.JournalpostPeriodJournaldato:
                case SokFelt.DokumentbeskrivelsePeriodOpprettetDato:
                case SokFelt.MappePeriodOpprettetDato:
                case SokFelt.MappePeriodAvsluttetDato:
                case SokFelt.DokumentbeskrivelsePeriodSkjermingPeriodSkjermingOpphoererDato:
                case SokFelt.RegistreringPeriodOpprettetDato:
                case SokFelt.SakPeriodSaksdato:
                case SokFelt.RegistreringPeriodSkjermingPeriodSkjermingOpphoererDato:
                case SokFelt.MappePeriodSkjermingPeriodSkjermingOpphoererDato:    
                case SokFelt.JournalpostPeriodSaksaar:
                case SokFelt.SakPeriodSaksaar:
                case SokFelt.JournalpostPeriodJournalaar:
                    return true;
                default:
                    return false;
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
