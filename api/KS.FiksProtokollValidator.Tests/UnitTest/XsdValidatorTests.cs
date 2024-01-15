#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation;
using Xunit;

namespace KS.FiksProtokollValidator.Tests.UnitTest
{
    public class XsdValidatorTests
    {
        [Fact]
        public void ValidateSokeresultatNoekler()
        {
            var xsdValidator = new XsdValidator();
            var validationErrors = new List<string>();
            var sokeresultat = File.ReadAllText(Directory.GetCurrentDirectory() + "/TestData/Responses/sokeresultatNoekler.xml");
            xsdValidator.Validate(sokeresultat, validationErrors);
            foreach (var validationError in validationErrors)
            {
                Console.Out.WriteLine(validationError);
            }
            Assert.Empty(validationErrors);
        }
        
        [Fact]
        public void ValidateSokeresultatMinimum()
        {
            var xsdValidator = new XsdValidator();
            var validationErrors = new List<string>();
            var sokeresultat = File.ReadAllText(Directory.GetCurrentDirectory() + "/TestData/Responses/sokeresultatMinimum.xml");
            xsdValidator.Validate(sokeresultat, validationErrors);
            foreach (var validationError in validationErrors)
            {
                Console.Out.WriteLine(validationError);
            }
            Assert.Empty(validationErrors);
        }
        
        [Fact]
        public void ValidateSokeresultatUtvidet()
        {
            var xsdValidator = new XsdValidator();
            var validationErrors = new List<string>();
            var sokeresultat = File.ReadAllText(Directory.GetCurrentDirectory() + "/TestData/Responses/sokeresultatUtvidet.xml");
            xsdValidator.Validate(sokeresultat, validationErrors);
            foreach (var validationError in validationErrors)
            {
                Console.Out.WriteLine(validationError);
            }
            Assert.Empty(validationErrors);
        }
    }
}