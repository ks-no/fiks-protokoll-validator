#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace KS.FiksProtokollValidator.Tests.UnitTest.Protocols.FiksPlan.TestCases
{
    public class TestCasesTests : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly JsonValidator _jsonValidator;
        public TestCasesTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _jsonValidator = JsonValidator.Init().WithFiksPlan();

        }
        
        [Fact]
        public void FiksPlan_TestCases_Are_Valid()
        {
            List<string> validationErrors = [];
            
            var testCasesDirectories = Directory.GetDirectories("TestCases/no.ks.fiks.plan.v2");
            foreach (var testCaseDir in testCasesDirectories)
            {
                var testInformationJson = File.ReadAllText($"{testCaseDir}/testInformation.json");
                var testInformation = JObject.Parse(testInformationJson);
                var supported = (testInformation.SelectToken("supported") ?? throw new InvalidOperationException()).Value<bool>();
                var messageType = (testInformation.SelectToken("messageType") ?? "").Value<string>();
                var samplePath = (testInformation.SelectToken("samplePath") ?? "").Value<string>();
                var testCaseName = testCaseDir.Substring(testCaseDir.LastIndexOf("/", StringComparison.Ordinal) + 1);
                
                if (!supported)
                {
                    _testOutputHelper.WriteLine($"Skipping unsupported testcase {testCaseName}");
                    continue;
                }

                var json = "";
                if (samplePath != null)
                {
                    json = File.ReadAllText($"{samplePath}/payload.json");
                }
                else
                {
                    json = File.ReadAllText($"{testCaseDir}/payload.json");
                }

                _testOutputHelper.WriteLine($"Validating testcase {testCaseName}");

                _jsonValidator.Validate(json, validationErrors, messageType);
                foreach (var validationError in validationErrors)
                {
                    _testOutputHelper.WriteLine(validationError);
                }
                Assert.Empty(validationErrors);
            }
        }

        public void Dispose()
        {
            _jsonValidator.Dispose();
        }
    }
}