using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KS.Fiks.IO.Arkiv.Client.Models;
using KS.Fiks.IO.Client.Models.Feilmelding;
using KS.FiksProtokollValidator.WebAPI.Models;
using KS.FiksProtokollValidator.WebAPI.Validation;
using NUnit.Framework;

namespace KS.FiksProtokollValidator.Tests
{
    public class Tests
    {
        private IFiksResponseValidator _validator;
        private FiksResponseTest _fiksResponseTest;
        private TestCase _testCase;
        private FiksResponse _fiksResponseMottatt;
        private FiksResponse _fiksResponseKvittering;
        private FiksRequest _fiksRequest;
        private TestSession _testSession;

        [SetUp]
        public void Setup()
        {
            _validator = new FiksResponseValidator();

            _fiksResponseTest = new FiksResponseTest
            {
                PayloadQuery = "/arkivmelding/registrering",
                ExpectedValue = "someValue",
                ValueType = SearchValueType.Attribute,
            };

            var requestPayloadFilePath = "./TestData/Requests/ny_inngaaende.xml";

            _testCase = new TestCase
            {
                MessageType = ArkivintegrasjonMeldingTypeV1.BasisArkivmelding,
                TestName = "testTestCase",
                FiksResponseTests = new List<FiksResponseTest>(),
                PayloadFileName = requestPayloadFilePath,
                ExpectedResponseMessageTypes = new List<FiksExpectedResponseMessageType>() { new FiksExpectedResponseMessageType() { ExpectedResponseMessageType = "no.ks.fiks.gi.arkivintegrasjon.mottatt.v1" }, new FiksExpectedResponseMessageType() { ExpectedResponseMessageType = "no.ks.fiks.gi.arkivintegrasjon.kvittering.v1" } }
            };

            _testCase.FiksResponseTests.Add(_fiksResponseTest);

            _fiksResponseMottatt = new FiksResponse
            {
                Type = ArkivintegrasjonMeldingTypeV1.Mottatt,
            };

            var responsePayloadFilePath = "./TestData/Responses/svar_paa_ny_inngaaende.xml";

            byte[] fileAsBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                Stream s = File.OpenRead(responsePayloadFilePath);
                s.CopyTo(ms);
                fileAsBytes = ms.ToArray();
            }

            _fiksResponseKvittering = new FiksResponse
            {
                Type = ArkivintegrasjonMeldingTypeV1.Kvittering,
                ReceivedAt = DateTime.Now,
                FiksPayloads = new List<FiksPayload> { new FiksPayload() { Filename = "svar_paa_ny_inngaaende.xml", Payload = fileAsBytes } },
            };

            _fiksRequest = new FiksRequest
            {
                MessageGuid = new Guid("F15D3D0D-FA20-41D7-B762-A718ACE95A0B"),
                FiksResponses = new List<FiksResponse>(),
                SentAt = DateTime.Now,
                IsFiksResponseValidated = false,
                FiksResponseValidationErrors = new List<string>(),
                TestCase = _testCase,
            };

            _fiksRequest.FiksResponses.Add(_fiksResponseMottatt);
            _fiksRequest.FiksResponses.Add(_fiksResponseKvittering);

            _testSession = new TestSession
            {
                Id = new Guid("0459C8B6-EAF1-4186-8FCB-BEC3AC311404"),
                FiksRequests = new List<FiksRequest>(),
                CreatedAt = DateTime.Now,
            };

            _testSession.FiksRequests.Add(_fiksRequest);
        }

        [Test]
        public void NonExistingNodeIsReported()
        {
            _fiksResponseTest.PayloadQuery = "/denne/noden/eksisterer/ikke";
            _fiksResponseTest.ExpectedValue = "*";
            _fiksResponseTest.ValueType = SearchValueType.Value;

            _validator.Validate(_testSession);

            var expectedMessage = string.Format(
                WebAPI.Validation.Resources.ValidationErrorMessages.MissingPayloadElement,
                _fiksResponseTest.PayloadQuery
            );

            Assert.Contains(expectedMessage, _fiksRequest.FiksResponseValidationErrors);
        }

        [Test]
        public void NotFoundAttributeIsReported()
        {
            _fiksResponseTest.ExpectedValue = "indianer";
            _fiksResponseTest.ValueType = SearchValueType.Attribute;

            _validator.Validate(_testSession);

            var xmlNodeToLookAt = _fiksResponseTest.PayloadQuery.Split('/').Last();

            var expectedMessage = string.Format(
                WebAPI.Validation.Resources.ValidationErrorMessages.MissingAttributeOnPayloadElement,
                _fiksResponseTest.ExpectedValue, xmlNodeToLookAt);

            Assert.Contains(expectedMessage, _fiksRequest.FiksResponseValidationErrors);
        }

        [Test]
        public void ExistingAttributeIsFound()
        {
            _fiksResponseTest.ExpectedValue = "journalpost";
            _fiksResponseTest.ValueType = SearchValueType.Attribute;

            var customTest = new FiksResponseTest
            {
                PayloadQuery = "/arkivmelding/registrering",
                ExpectedValue = "apekatt",
                ValueType = SearchValueType.Attribute,
            };

            _testCase.FiksResponseTests.Add(customTest);

            _validator.Validate(_testSession);

            var xmlNodeToLookAt = _fiksResponseTest.PayloadQuery.Split('/').Last();

            var expectedMessage = string.Format(
                WebAPI.Validation.Resources.ValidationErrorMessages.MissingAttributeOnPayloadElement,
                customTest.ExpectedValue, xmlNodeToLookAt);

            Assert.Contains(expectedMessage, _fiksRequest.FiksResponseValidationErrors);

            _fiksRequest.FiksResponseValidationErrors.Remove(expectedMessage);

            Assert.IsEmpty(_fiksRequest.FiksResponseValidationErrors);
        }
    }
}