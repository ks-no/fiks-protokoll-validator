#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace KS.FiksProtokollValidator.Tests.UnitTest.WebAPI.TjenerValidator.Validation
{
    public class PayloadValidatorTests : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly PayloadValidator _payloadValidator;
        
        public PayloadValidatorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _payloadValidator = new PayloadValidator();

        }
        
        [Fact]
        public void FiksSaksfaser_Saksfaser_Hent_Resultat_Is_Valid()
        {
            var validationErrors = new List<string>();
            var payload = File.ReadAllText("TestData/Saksfaser/no.ks.fiks.saksfaser.v1.saksfaser.hent.resultat.json");
            var responseMessageType = "no.ks.fiks.saksfaser.v1.saksfaser.hent.resultat";
            _payloadValidator.ValidateJsonWithSchema(payload, validationErrors, responseMessageType);
            Assert.Empty(validationErrors);
        }

        [Fact]
        public void FiksSaksfaser_Saksfaser_Hent_Resultat_Is_Not_Valid()
        {
            var validationErrors = new List<string>();
            var payload = File.ReadAllText("TestData/Saksfaser/no.ks.fiks.saksfaser.v1.saksfaser.hent.resultat-not-valid.json");
            var responseMessageType = "no.ks.fiks.saksfaser.v1.saksfaser.hent.resultat";
            _payloadValidator.ValidateJsonWithSchema(payload, validationErrors, responseMessageType);
            Assert.NotEmpty(validationErrors);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}