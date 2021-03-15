using System.Collections.Generic;
using System.IO;
using System.Linq;
using KS.FiksProtokollValidator.WebAPI.Models;
using Newtonsoft.Json.Linq;

namespace KS.FiksProtokollValidator.WebAPI.Data
{
    public class TestSeeder : ITestSeeder
    {
        private readonly FiksIOMessageDBContext _context;

        public TestSeeder(FiksIOMessageDBContext context)
        {
            _context = context;

            Seed();
        }

        private void Seed()
        {
            var tests = new DirectoryInfo(@"TestCases/");

            foreach (var testDirectory in tests.GetDirectories())
            {
                var testInformationJson =
                    File.ReadAllText(Path.Combine(testDirectory.FullName, "testInformation.json"));
                var testInformation = JObject.Parse(testInformationJson);

                var testName = testDirectory.Name;

                if (TestExistInDatabase(testName))
                    continue;

                var test = new TestCase
                {
                    TestName = (string)testInformation["testName"],
                    MessageType = (string) testInformation["messageType"],
                    PayloadFileName = "arkivmelding.xml",
                    FiksResponseTests = new List<FiksResponseTest>(),
                    Description = (string) testInformation["description"],
                    TestStep = (string) testInformation["testStep"],
                    Operation = (string) testInformation["operation"],
                    Situation = (string) testInformation["situation"],
                    ExpectedResult = (string) testInformation["expectedResult"]
                };

                var attachmentDirectory = Path.Combine(testDirectory.FullName, "Attachments");
                if (Directory.Exists(attachmentDirectory))
                {
                    var payloadAttachmentFileNames = "";

                    foreach (var fileInfo in new DirectoryInfo(attachmentDirectory)
                        .GetFiles())
                    {
                        payloadAttachmentFileNames += fileInfo.Name + ";";
                    }

                    test.PayloadAttachmentFileNames = payloadAttachmentFileNames.TrimEnd(';');
                }

                foreach (var queryWithExpectedValue in testInformation["queriesWithExpectedValues"])
                {
                    var fiksResponseTest = new FiksResponseTest
                    {
                        PayloadQuery = (string) queryWithExpectedValue["payloadQuery"],
                        ExpectedValue = (string) queryWithExpectedValue["expectedValue"],
                        ValueType = (SearchValueType) (int) queryWithExpectedValue["valueType"]
                    };

                    test.FiksResponseTests.Add(fiksResponseTest);
                }

                _context.TestCases.Add(test);
                _context.SaveChanges();
            }
        }

        private bool TestExistInDatabase(string testName)
        {
            return _context.TestCases.Any(a => (a.Operation+a.Situation).Equals(testName));
        }
    }
}