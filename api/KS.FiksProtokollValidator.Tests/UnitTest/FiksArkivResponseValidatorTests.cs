using System;
using System.Collections.Generic;
using System.IO;
using KS.Fiks.Arkiv.Models.V1.Meldingstyper;
using KS.FiksProtokollValidator.WebAPI.Models;
using KS.FiksProtokollValidator.WebAPI.Validation;
using NUnit.Framework;

namespace KS.FiksProtokollValidator.Tests.UnitTest
{
    public class FiksArkivResponseValidatorTests
    {
        [Test]
        public void TestValidateSokOnMappeTittelXmlWithCorrectResult()
        {
            var responseXml = File.ReadAllText(Directory.GetCurrentDirectory() + "/TestData/ValidatorTests/NySokN1/sokeresultatMinimum.xml");
            var validationErrors = new List<string>();

            var fiksRequest = new FiksRequest
            {
                TestCase = new TestCase
                {
                    MessageType = FiksArkivMeldingtype.Sok,
                    PayloadFileName = "sok.xml",
                    SamplePath = "/TestData/ValidatorTests/NySokN1"
                }
            };
            
            FiksResponseValidator.ValidateXmlPayloadContent(responseXml, fiksRequest, validationErrors);
            foreach (var validationError in validationErrors)
            {
                Console.Out.WriteLine(validationError);
            }
            Assert.True(validationErrors.Count == 0);
        }

        [Test]
        public void TestValidateSokBetweenDatesXmlWithCorrectResult()
        {
            var responseXml = File.ReadAllText(Directory.GetCurrentDirectory() + "/TestData/ValidatorTests/NySokN4/sokeresultatMinimum.xml");
            var validationErrors = new List<string>();

            var fiksRequest = new FiksRequest
            {
                TestCase = new TestCase
                {
                    MessageType = FiksArkivMeldingtype.Sok,
                    PayloadFileName = "sok.xml",
                    SamplePath = "/TestData/ValidatorTests/NySokN4"
                }
            };
            
            FiksResponseValidator.ValidateXmlPayloadContent(responseXml, fiksRequest, validationErrors);
            foreach (var validationError in validationErrors)
            {
                Console.Out.WriteLine(validationError);
            }
            Assert.True(validationErrors.Count == 0);
        }
    }
}