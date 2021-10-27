using System.Collections.Generic;
using System.IO;
using System.Linq;
using KS.FiksProtokollValidator.WebAPI.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json.Linq;

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

            foreach (var protocolDirectory in tests.GetDirectories())
            {
                foreach (var testDirectory in protocolDirectory.GetDirectories())
                {
                    var testInformationJson =
                        File.ReadAllText(Path.Combine(testDirectory.FullName, "testInformation.json"));
                    var testInformation = JObject.Parse(testInformationJson);

                    var updateTest = _context.TestCases.Where(t => t.TestName == (string)testInformation["testName"])
                        .Include(t => t.ExpectedResponseMessageTypes).Include(t => t.FiksResponseTests).FirstOrDefault();
                    if (updateTest != null)
                    {
                        _context.TestCases.Update(UpdateTest(testDirectory, updateTest, testInformation));
                    }
                    else
                    {
                        var newTestEntry = new TestCase();
                        newTestEntry.TestName = (string)testInformation["testName"];
                        _context.TestCases.Add(UpdateTest(testDirectory, newTestEntry, testInformation));
                    }
                    _context.SaveChanges();
                }
            }
        }

        private TestCase UpdateTest(DirectoryInfo testDirectory, TestCase testCase, JObject testInformation)
        {
            testCase.MessageType = (string)testInformation["messageType"];
            testCase.Description = (string)testInformation["description"];
            testCase.TestStep = (string)testInformation["testStep"];
            testCase.Operation = (string)testInformation["operation"];
            testCase.Situation = (string)testInformation["situation"];
            testCase.ExpectedResult = (string)testInformation["expectedResult"];
            testCase.Supported = (bool)testInformation["supported"];
            testCase.Protocol = testInformation["protocol"] == null ? "" : (string)testInformation["protocol"];

            if (!string.IsNullOrEmpty((string) testInformation["sampleFile"]))
            {
                var sampleFile = (string) testInformation["sampleFile"];
                var fileName = sampleFile.Split('/').Last();
                testCase.PayloadFileName = fileName;
                testCase.PayloadFilePath = sampleFile;
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
                    testCase.PayloadFilePath = testCaseDirectory + "/" + fileInfo.Name;
                }
            }

            var attachmentDirectory = Path.Combine(testDirectory.FullName, "Attachments");
            
            if (Directory.Exists(attachmentDirectory))
            {
                var payloadAttachmentFileNames = "";

                foreach (var fileInfo in new DirectoryInfo(attachmentDirectory)
                    .GetFiles())
                {
                    payloadAttachmentFileNames += fileInfo.Name + ";";
                }

                testCase.PayloadAttachmentFileNames = payloadAttachmentFileNames.TrimEnd(';');
            }

            // Legg til QueriesWithExpectedValues
            //TODO lag en DeleteQueriesWithExpectedValues
            AddQueriesWithExpectedValues(testCase, testInformation);

            // Slett og legg til expectedResponseMessageTypes
            DeleteFiksExpectedResponseMessageTypes(testCase, testInformation);
            AddFiksExpectedResponseMessageTypes(testCase, testInformation);

            return testCase;
        }

        private static void AddQueriesWithExpectedValues(TestCase testCase, JObject testInformation)
        {
            if (testInformation["queriesWithExpectedValues"] == null)
            {
                return;
            }
            
            if (testCase.FiksResponseTests == null)
            {
                testCase.FiksResponseTests = new List<FiksResponseTest>();
                foreach (var queryWithExpectedValue in testInformation["queriesWithExpectedValues"])
                {
                    var fiksResponseTest = new FiksResponseTest
                    {
                        PayloadQuery = (string) queryWithExpectedValue["payloadQuery"],
                        ExpectedValue = (string) queryWithExpectedValue["expectedValue"],
                        ValueType = (SearchValueType) (int) queryWithExpectedValue["valueType"]
                    };

                    testCase.FiksResponseTests.Add(fiksResponseTest);
                }
            }
            else
            {
                foreach (var queryWithExpectedValue in testInformation["queriesWithExpectedValues"])
                {
                    var fiksResponseTest = new FiksResponseTest
                    {
                        PayloadQuery = (string) queryWithExpectedValue["payloadQuery"],
                        ExpectedValue = (string) queryWithExpectedValue["expectedValue"],
                        ValueType = (SearchValueType) (int) queryWithExpectedValue["valueType"]
                    };
                    if (!testCase.FiksResponseTests.Any(
                        r => (r.ExpectedValue.Equals(fiksResponseTest.ExpectedValue)
                              && r.PayloadQuery.Equals(fiksResponseTest.PayloadQuery)
                              && r.ValueType.Equals(fiksResponseTest.ValueType))
                    ))
                    {
                        testCase.FiksResponseTests.Add(fiksResponseTest);
                    }
                }
            }
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