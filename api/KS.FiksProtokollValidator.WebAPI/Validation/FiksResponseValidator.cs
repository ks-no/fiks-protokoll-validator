using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using KS.FiksProtokollValidator.WebAPI.Models;
using KS.FiksProtokollValidator.WebAPI.Validation.Resources;
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
                    ExpectedResponseMessageTypeProvider.GetExpectedResponseMessageTypes(
                        fiksRequest.TestCase.MessageType, fiksRequest.TestCase.ExpectedResponseMessageTypes
                    );

                    fiksRequest.FiksResponseValidationErrors = new List<string>();

                ValidateExistenceOfExpectedResponseMessageTypes(fiksRequest, expectedResponseMessageTypes);

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
            var receivedPayloadFileName = fiksResponse.Payload;
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

            if (receivedPayloadFileName != null && !receivedPayloadFileName.EndsWith(".xml"))
            {
                validationErrors.Add(string.Format(
                    ValidationErrorMessages.InvalidPayloadFileFormatMessage, receivedPayloadFileName.Split('.').Last()
                ));
                return;
            }

            ValidateXmlPayloadContent(fiksResponse.PayloadContent, fiksResponseTests, validationErrors);
        }

        private static bool ResponseMessageShouldHavePayload(string responseMessageType)
        {
            _messageTypesWithPayloads ??= GetMessageTypesWithPayload();
            return _messageTypesWithPayloads.Contains(responseMessageType);
        }

        private static HashSet<string> GetMessageTypesWithPayload()
        {
            return new HashSet<string>
            {
                WebAPI.Resources.ResponseMessageTypes.KvitteringV1,
                WebAPI.Resources.ResponseMessageTypes.InnsynSoekResultatV1,
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
    }
}
