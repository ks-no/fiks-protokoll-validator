using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KS.FiksProtokollValidator.WebAPI.Models;
using KS.FiksProtokollValidator.WebAPI.Resources;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json.Linq;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.Data
{
    public class TestSeeder : ITestSeeder
    {
        private readonly FiksIOMessageDBContext _context;
        public const string TestsDirectory = "TestCases/";

        public TestSeeder(FiksIOMessageDBContext context)
        {
            _context = context;

            Seed();
        }

        private void Seed()
        {
            var tests = new DirectoryInfo(@"TestCases/");

            // Slett alt innhold i FiksResponseTest og bygg den opp fra scratch fra TestCase filene. Dette er mer effektivt og brekker ikke noen avhengigheter
            _context.Database.ExecuteSqlRaw("TRUNCATE TABLE FiksResponseTest");

            foreach (var protocolDirectory in tests.GetDirectories())
            {
                var protocolName = protocolDirectory.Name;
                foreach (var testDirectory in protocolDirectory.GetDirectories())
                {
                    var testDirectoryName = testDirectory.Name;
                    var testInformationJson =
                        File.ReadAllText(Path.Combine(testDirectory.FullName, "testInformation.json"));
                    var testInformation = JObject.Parse(testInformationJson);

                    var testId = $"{protocolName}-{testDirectoryName}";

                    var updateTest = _context.TestCases.Where(t => t.TestId == testId)
                        .Include(t => t.ExpectedResponseMessageTypes).Include(t => t.FiksResponseTests).FirstOrDefault();
                    
                    
                    if (updateTest != null)
                    {
                        Log.Information("Update test with id {TestCaseId}", testId);
                        _context.TestCases.Update(UpdateTest(testDirectory, updateTest, testInformation));
                    }
                    else
                    {
                        Log.Information("Create test with id {TestCaseId}", testId);
                        var newTestEntry = new TestCase {TestId = testId};
                        _context.TestCases.Add(UpdateTest(testDirectory, newTestEntry, testInformation));
                    }
                    _context.SaveChanges();
                }
            }
        }

        private TestCase UpdateTest(DirectoryInfo testDirectory, TestCase testCase, JObject testInformation)
        {
            testCase.TestName = (string) testInformation["testName"];
            testCase.MessageType = (string)testInformation["messageType"];
            testCase.Description = (string)testInformation["description"];
            testCase.TestStep = (string)testInformation["testStep"];
            testCase.Operation = (string)testInformation["operation"];
            testCase.Situation = (string)testInformation["situation"];
            testCase.ExpectedResult = (string)testInformation["expectedResult"];
            testCase.Supported = (bool)testInformation["supported"];
            testCase.Protocol = testInformation["protocol"] == null ? "" : (string)testInformation["protocol"];

            var fileName = PayloadNames.Dictionary[testCase.Protocol];
            testCase.PayloadFileName = fileName;

            if (!string.IsNullOrEmpty((string) testInformation["samplePath"]))
            {
                var samplePath = (string) testInformation["samplePath"];
                testCase.SamplePath = samplePath;


                var basepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                string attachmentDirectory = Path.Combine(samplePath, "Attachments");

                var fullAttachementPath = Path.Combine(basepath, attachmentDirectory);

                testCase = SupplyAttachents(testCase, fullAttachementPath);
            }
            else
            {
                foreach (var fileInfo in new DirectoryInfo(testDirectory.FullName)
                    .GetFiles())
                {
                    if (fileInfo.Name.Equals("testInformation.json"))
                    {
                        continue;
                    }
                    
                    testCase.PayloadFileName = fileInfo.Name;
                    var testCaseDirectory = Path.Combine(TestsDirectory, testCase.Protocol, testCase.Operation + testCase.Situation);
                    testCase.SamplePath = testCaseDirectory;
                }
                var attachmentDirectory = Path.Combine(testDirectory.FullName, "Attachments");
                testCase = SupplyAttachents(testCase, attachmentDirectory);
            }

            // Legg til QueriesWithExpectedValues
            //TODO lag en DeleteQueriesWithExpectedValues
            AddQueriesWithExpectedValues(testCase, testInformation);

            // Slett og legg til expectedResponseMessageTypes
            DeleteFiksExpectedResponseMessageTypes(testCase, testInformation);
            AddFiksExpectedResponseMessageTypes(testCase, testInformation);

            return testCase;
        }

        private TestCase SupplyAttachents(TestCase testCase, string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                var payloadAttachmentFileNames = "";

                foreach (var fileInfo in new DirectoryInfo(directoryPath)
                    .GetFiles())
                {
                    payloadAttachmentFileNames += fileInfo.Name + ";";
                }

                testCase.PayloadAttachmentFileNames = payloadAttachmentFileNames.TrimEnd(';');
            }
            else if (!string.IsNullOrEmpty(testCase.PayloadAttachmentFileNames))
            {
                testCase.PayloadAttachmentFileNames = null;
            }
            return testCase;
        }

        private static void AddQueriesWithExpectedValues(TestCase testCase, JObject testInformation)
        {
            if (testInformation["queriesWithExpectedValues"] == null)
            {
                return;
            }
            
            testCase.FiksResponseTests ??= new List<FiksResponseTest>();
            foreach (var queryWithExpectedValue in testInformation["queriesWithExpectedValues"])
            {
                AddNewFiksResponseTest(testCase, queryWithExpectedValue);
            }
        }

        private static void AddNewFiksResponseTest(TestCase testCase, JToken queryWithExpectedValue)
        {
            var fiksResponseTest = new FiksResponseTest
            {
                PayloadQuery = (string)queryWithExpectedValue["payloadQuery"],
                ExpectedValue = (string)queryWithExpectedValue["expectedValue"],
                ValueType = (SearchValueType)(int)queryWithExpectedValue["valueType"]
            };

            testCase.FiksResponseTests.Add(fiksResponseTest);
        }

        private void DeleteFiksExpectedResponseMessageTypes(TestCase testCase, JObject testInformation)
        {
            // Delete all?
            if ((testInformation["expectedResponseMessageTypes"] == null || !testInformation["expectedResponseMessageTypes"].HasValues) && testCase.ExpectedResponseMessageTypes?.Count > 0)
            {
                foreach (var dbExpMsgRspType in testCase.ExpectedResponseMessageTypes)
                {
                    _context.Entry(dbExpMsgRspType).State = EntityState.Deleted;
                }
                testCase.ExpectedResponseMessageTypes = null;
                return;
            }

            if (testCase.ExpectedResponseMessageTypes == null)
            {
                return;
            }
            
            // Find what to delete
            var i = 0;
            var deleteIndexes = new List<int>(); 
            
            foreach (var dbTestCase in testCase.ExpectedResponseMessageTypes)
            {
                var found = testInformation["expectedResponseMessageTypes"].Any(fileTestCase => fileTestCase.ToString() == dbTestCase.ExpectedResponseMessageType);
                if (!found)
                {
                    deleteIndexes.Add(i);
                }
                i++;
            }

            foreach (var deleteIndex in deleteIndexes)
            {
                var dbExpMsgRspType = testCase.ExpectedResponseMessageTypes[deleteIndex];
                _context.Entry(dbExpMsgRspType).State = EntityState.Deleted;
                testCase.ExpectedResponseMessageTypes.RemoveAt(deleteIndex);
            }
        }

        private void AddFiksExpectedResponseMessageTypes(TestCase testCase, JObject testInformation)
        {
            if (testCase.ExpectedResponseMessageTypes == null)
            {
                testCase.ExpectedResponseMessageTypes = new List<FiksExpectedResponseMessageType>();
                if (testInformation.ContainsKey("expectedResponseMessageTypes"))
                {
                    foreach (var messageType in testInformation["expectedResponseMessageTypes"])
                    {
                        var fiksExpectedResponseMessageType = new FiksExpectedResponseMessageType
                        {
                            ExpectedResponseMessageType = (string)messageType
                        };
                        testCase.ExpectedResponseMessageTypes.Add(fiksExpectedResponseMessageType);
                    }
                }
                
            }
            else if(testInformation["expectedResponseMessageTypes"] != null && testInformation["expectedResponseMessageTypes"].HasValues)
            {
                foreach (var messageType in testInformation["expectedResponseMessageTypes"])
                {
                    var fiksExpectedResponseMessageType = new FiksExpectedResponseMessageType
                    {
                        ExpectedResponseMessageType = (string) messageType
                    };
                    if (!testCase.ExpectedResponseMessageTypes.Any(r =>
                        r.ExpectedResponseMessageType.Equals(fiksExpectedResponseMessageType.ExpectedResponseMessageType)))
                    {
                        testCase.ExpectedResponseMessageTypes.Add(fiksExpectedResponseMessageType);
                    }
                }
            }
        }
    }
}