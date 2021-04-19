using System.Collections.Generic;
using System.IO;
using System.Linq;
using KS.FiksProtokollValidator.WebAPI.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
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

                var updateTest = _context.TestCases.Where(t => t.TestName == (string)testInformation["testName"])
                    .Include(t => t.ExpectedResponseMessageTypes).Include(t=> t.FiksResponseTests).FirstOrDefault();
                if (updateTest != null)
                {
                    _context.TestCases.Update(UpdateTest(testDirectory, updateTest, testInformation));
                }
                else
                {
                    var newTestEntry = new TestCase();
                    newTestEntry.TestName = (string) testInformation["testName"];
                    _context.TestCases.Add(UpdateTest(testDirectory, newTestEntry, testInformation));
                }
                _context.SaveChanges();
            }
        }

        private TestCase UpdateTest(DirectoryInfo testDirectory, TestCase testCase, JObject testInformation)
        {
            testCase.MessageType = (string)testInformation["messageType"];


            foreach (var fileInfo in new DirectoryInfo(testDirectory.FullName)
                    .GetFiles())
            {
                if (!fileInfo.Name.Equals("testInformation.json"))
                {
                    testCase.PayloadFileName = fileInfo.Name;
                }
            }

            testCase.Description = (string)testInformation["description"];
            testCase.TestStep = (string)testInformation["testStep"];
            testCase.Operation = (string)testInformation["operation"];
            testCase.Situation = (string)testInformation["situation"];
            testCase.ExpectedResult = (string)testInformation["expectedResult"];
            testCase.Supported = (bool)testInformation["supported"];
            testCase.Protocol = testInformation["protocol"] == null ? "":(string)testInformation["protocol"];

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

            if (testInformation["queriesWithExpectedValues"] != null)
            {
                if (testCase.FiksResponseTests == null)
                {
                    testCase.FiksResponseTests = new List<FiksResponseTest>();
                    foreach (var queryWithExpectedValue in testInformation["queriesWithExpectedValues"])
                    {
                        var fiksResponseTest = new FiksResponseTest
                        {
                            PayloadQuery = (string)queryWithExpectedValue["payloadQuery"],
                            ExpectedValue = (string)queryWithExpectedValue["expectedValue"],
                            ValueType = (SearchValueType)(int)queryWithExpectedValue["valueType"]
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
                            PayloadQuery = (string)queryWithExpectedValue["payloadQuery"],
                            ExpectedValue = (string)queryWithExpectedValue["expectedValue"],
                            ValueType = (SearchValueType)(int)queryWithExpectedValue["valueType"]
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

            if (testInformation["expectedResponseMessageTypes"] != null)
            {
                if (testCase.ExpectedResponseMessageTypes == null)
                {
                    testCase.ExpectedResponseMessageTypes = new List<FiksExpectedResponseMessageType>();
                    foreach (var messageType in testInformation["expectedResponseMessageTypes"])
                    {
                        var fiksExpectedResponseMessageType = new FiksExpectedResponseMessageType
                        {
                            ExpectedResponseMessageType = (string)messageType
                        };
                        testCase.ExpectedResponseMessageTypes.Add(fiksExpectedResponseMessageType);
                    }
                }
                else
                {
                    foreach (var messageType in testInformation["expectedResponseMessageTypes"])
                    {
                        var fiksExpectedResponseMessageType = new FiksExpectedResponseMessageType
                        {
                            ExpectedResponseMessageType = (string)messageType
                        };
                        if (!testCase.ExpectedResponseMessageTypes.Any(r=> r.ExpectedResponseMessageType.Equals(fiksExpectedResponseMessageType.ExpectedResponseMessageType)))
                        {
                            testCase.ExpectedResponseMessageTypes.Add(fiksExpectedResponseMessageType);
                        }
                    }
                }
                
            }
            return testCase;
        }
    }
}