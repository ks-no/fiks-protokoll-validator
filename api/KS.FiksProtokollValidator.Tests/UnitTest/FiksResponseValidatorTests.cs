using System;
using System.Collections.Generic;
using System.IO;
using KS.Fiks.IO.Arkiv.Client.Models;
using KS.FiksProtokollValidator.WebAPI.Models;
using KS.FiksProtokollValidator.WebAPI.Validation;
using NUnit.Framework;

namespace KS.FiksProtokollValidator.Tests.UnitTest
{
    public class FiksResponseValidatorTests
    {
        [Test]
        public void TestValidateSokXmlWithCorrectResult()
        {
            var responseXml = File.ReadAllText(Directory.GetCurrentDirectory() + "/TestData/ValidatorTests/NySokN1/sokeresultatMinimum.xml");
            var validationErrors = new List<string>();

            var fiksRequest = new FiksRequest
            {
                TestCase = new TestCase
                {
                    MessageType = ArkivintegrasjonMeldingTypeV1.Sok,
                    PayloadFilePath = "/TestData/ValidatorTests/NySokN1/sok.xml"
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
        public void TestValidateSokXmlWithWrongResult()
        {
            var responseXml = File.ReadAllText(Directory.GetCurrentDirectory() + "/TestData/ValidatorTests/NySokN1/sokeresultatMinimum2.xml");
            var validationErrors = new List<string>();

            var fiksRequest = new FiksRequest
            {
                TestCase = new TestCase
                {
                    MessageType = ArkivintegrasjonMeldingTypeV1.Sok,
                    PayloadFilePath = "/TestData/ValidatorTests/NySokN1/sok2.xml"
                }
            };
            
            FiksResponseValidator.ValidateXmlPayloadContent(responseXml, fiksRequest, validationErrors);
            foreach (var validationError in validationErrors)
            {
                Console.Out.WriteLine(validationError);
            }
            Assert.True(validationErrors.Count > 0);
        }
    }
}