#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace KS.FiksProtokollValidator.Tests.UnitTest.FiksArkiv
{
    public class TestCasesTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        public TestCasesTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
        
        [Fact]
        public void FiksArkiv_TestCases_Are_Valid()
        {
            var xsdValidator = new XsdValidator();
            List<string> validationErrors = new List<string>();
            
            var testCasesShouldBeInvalid = new[]{"NySaksmappeFX"};
            if (testCasesShouldBeInvalid == null) throw new ArgumentNullException(nameof(testCasesShouldBeInvalid));

            var testCasesDirectories = Directory.GetDirectories("TestCases/no.ks.fiks.arkiv.v1");
            foreach (var testCaseDir in testCasesDirectories)
            {
                var testInformationJson = File.ReadAllText($"{testCaseDir}/testInformation.json");
                var testInformation = JObject.Parse(testInformationJson);
                var supported = (testInformation.SelectToken("supported") ?? throw new InvalidOperationException()).Value<bool>();
                var testCaseName = testCaseDir.Substring(testCaseDir.LastIndexOf("/", StringComparison.Ordinal) + 1);
                
                if (!supported)
                {
                    _testOutputHelper.WriteLine($"Skipping unsupported testcase {testCaseName}");
                    continue;
                }

                if (testCasesShouldBeInvalid.Contains(testCaseName))
                {
                    _testOutputHelper.WriteLine($"Skipping validating testcase that should be invalid {testCaseName}");
                    continue;
                }
                
                string xml;
                if (File.Exists($"{testCaseDir}/arkivmelding.xml"))
                {
                    xml = File.ReadAllText($"{testCaseDir}/arkivmelding.xml");
                }
                else if (File.Exists($"{testCaseDir}/mappe-hent.xml"))
                {
                    xml = File.ReadAllText($"{testCaseDir}/mappe-hent.xml");
                }
                else if (File.Exists($"{testCaseDir}/registrering-hent.xml"))
                {
                    xml = File.ReadAllText($"{testCaseDir}/registrering-hent.xml");
                }
                else
                {
                    xml = File.ReadAllText($"{testCaseDir}/sok.xml");
                }

                _testOutputHelper.WriteLine($"Validating testcase {testCaseName}");
                
                xsdValidator.Validate(xml, validationErrors);
                foreach (var validationError in validationErrors)
                {
                    _testOutputHelper.WriteLine(validationError);
                }
                Assert.Empty(validationErrors);
            }
        }
    }
}