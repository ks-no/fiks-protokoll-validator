#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KS.FiksProtokollValidator.WebAPI.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace KS.FiksProtokollValidator.Tests.UnitTest
{
    public class TestCasesTests
    {
        public TestCasesTests()
        {
        }
        
        [Test]
        public void FiksArkiv_TestCases_Are_Valid()
        {
            var xsdValidator = new XsdValidator();
            List<string> validationErrors = new List<string>();
            
            var TestCasesShouldBeInvalid = new[]{"NySaksmappeFX"};
            
            var TestCasesDirectories = Directory.GetDirectories("TestCases/no.ks.fiks.arkiv.v1");
            foreach (var testCaseDir in TestCasesDirectories)
            {
                var testInformationJson = File.ReadAllText($"{testCaseDir}/testInformation.json");
                var testInformation = JObject.Parse(testInformationJson);
                var supported = testInformation.SelectToken("supported").Value<bool>();
                var testCaseName = testCaseDir.Substring(testCaseDir.LastIndexOf("/") + 1);
                
                if (!supported)
                {
                    Console.Out.WriteLine($"Skipping unsupported testcase {testCaseName}");
                    continue;
                }

                if (TestCasesShouldBeInvalid.Contains(testCaseName))
                {
                    Console.Out.WriteLine($"Skipping validating testcase that should be invalid {testCaseName}");
                    continue;
                }
                
                string xml;
                if (File.Exists($"{testCaseDir}/arkivmelding.xml"))
                {
                    xml = File.ReadAllText($"{testCaseDir}/arkivmelding.xml");
                }
                else
                {
                    xml = File.ReadAllText($"{testCaseDir}/sok.xml");
                }

                Console.Out.WriteLine($"Validating testcase {testCaseName}");
                
                xsdValidator.Validate(xml, validationErrors);
                foreach (var validationError in validationErrors)
                {
                    Console.Out.WriteLine(validationError);
                }
                Assert.True(validationErrors.Count == 0);
            }
            
        }
        
        
    }
}