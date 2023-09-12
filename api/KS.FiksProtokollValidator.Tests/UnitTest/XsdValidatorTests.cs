#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation;
using NUnit.Framework;

namespace KS.FiksProtokollValidator.Tests.UnitTest
{
    public class XsdValidatorTests
    {
        public XsdValidatorTests()
        {
        }
        
        [Test]
        public void ValidateSokeresultatNoekler()
        {
            var xsdValidator = new XsdValidator();
            List<string> validationErrors = new List<string>();
            string sokeresultat = File.ReadAllText(Directory.GetCurrentDirectory() + "/TestData/Responses/sokeresultatNoekler.xml");
            xsdValidator.Validate(sokeresultat, validationErrors);
            foreach (var validationError in validationErrors)
            {
                Console.Out.WriteLine(validationError);
            }
            Assert.True(validationErrors.Count == 0);
        }
        
        [Test]
        public void ValidateSokeresultatMinimum()
        {
            var xsdValidator = new XsdValidator();
            List<string> validationErrors = new List<string>();
            string sokeresultat = File.ReadAllText(Directory.GetCurrentDirectory() + "/TestData/Responses/sokeresultatMinimum.xml");
            xsdValidator.Validate(sokeresultat, validationErrors);
            foreach (var validationError in validationErrors)
            {
                Console.Out.WriteLine(validationError);
            }
            Assert.True(validationErrors.Count == 0);
        }
        
        [Test]
        public void ValidateSokeresultatUtvidet()
        {
            var xsdValidator = new XsdValidator();
            List<string> validationErrors = new List<string>();
            string sokeresultat = File.ReadAllText(Directory.GetCurrentDirectory() + "/TestData/Responses/sokeresultatUtvidet.xml");
            xsdValidator.Validate(sokeresultat, validationErrors);
            foreach (var validationError in validationErrors)
            {
                Console.Out.WriteLine(validationError);
            }
            Assert.True(validationErrors.Count == 0);
        }
    }
}