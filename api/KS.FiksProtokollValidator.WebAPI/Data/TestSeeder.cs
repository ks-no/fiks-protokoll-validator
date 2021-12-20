using System.Collections.Generic;
using System.IO;
using System.Linq;
using KS.FiksProtokollValidator.WebAPI.Models;
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

                    //var updateTest = _context.TestCases.Where(t => t.TestName == (string)testInformation["testName"])
                    //    .Include(t => t.ExpectedResponseMessageTypes).Include(t => t.FiksResponseTests).FirstOrDefault();
                    
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
            
            //TODO søk gjennom TestCases og FiksResponseTest tabellene og slett eventuelle tester som ikke lenger eksisterer i mappene
            
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
            else if(!string.IsNullOrEmpty(testCase.PayloadAttachmentFileNames))
            {
                testCase.PayloadAttachmentFileNames = null;
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
        
        private void DeleteQueriesWithExpectedValues(TestCase testCase, JObject queryWithExpectedValue)
        {
            // Delete all?
            if ((queryWithExpectedValue["queriesWithExpectedValues"] == null || !queryWithExpectedValue["queriesWithExpectedValues"].HasValues) && testCase.FiksResponseTests?.Count > 0)
            {
                foreach (var dbFiksResponseTests in testCase.FiksResponseTests)
                {
                    _context.Entry(dbFiksResponseTests).State = EntityState.Deleted;
                }
                testCase.FiksResponseTests = null;
                return;
            }

            if (testCase.FiksResponseTests == null)
            {
                return;
            }
            
            // Find what to delete
            var deleteList = new List<FiksResponseTest>();       
            
            foreach (var fiksResponseTest in testCase.FiksResponseTests)
            {
                var found = false;
                foreach (var query in queryWithExpectedValue["queriesWithExpectedValues"])
                {
                    found = ((string) queryWithExpectedValue["payloadQuery"]).Equals(fiksResponseTest.PayloadQuery) 
                            && ((string) queryWithExpectedValue["expectedValue"]).Equals(fiksResponseTest.ExpectedValue)
                            && ((SearchValueType) (int) queryWithExpectedValue["valueType"]).Equals(fiksResponseTest.ValueType);
                   
                }

                if (found) { continue; }
                testCase.FiksResponseTests.Remove(fiksResponseTest);
                deleteList.Add(fiksResponseTest);
            }
            
            foreach (var fiksResponseTest in deleteList)
            {
                _context.Entry(fiksResponseTest).State = EntityState.Deleted;
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