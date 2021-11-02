using System;
using System.IO;
using System.Reflection;
using System.Web;
using KS.FiksProtokollValidator.WebAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using static System.IO.File;

namespace KS.FiksProtokollValidator.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCasePayloadFilesController : ControllerBase
    {
        private readonly FiksIOMessageDBContext _context;
        
        private const string TestsDirectoryPath = @"TestCases/";
        
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        public TestCasePayloadFilesController(FiksIOMessageDBContext context)
        {
            _context = context;
        }

        // GET api/<TestsCasePayloadFilesController>/TestCaseName
        [HttpGet("{protocol}/{testCaseName}/{fileName}")]
        public ActionResult GetMessagePayloadFile(string testCaseName, string protocol, string fileName)
        {
            var filePath = Path.Combine(TestsDirectoryPath, protocol, testCaseName, fileName);

            Log.Information("GetMessagePayloadFile get file for protocol {Protocol}, testCaseName {TestCaseName} with filePath {FilePath}", protocol, testCaseName, filePath);
            
            return GetPayload(filePath);
        }
        
        // GET api/<TestsCasePayloadFilesController>/TestCaseName
        [HttpGet("{protocol}/{testCaseName}/payload")]
        public ActionResult GetMessagePayloadFile(string testCaseName)
        {
            var decodedId = HttpUtility.UrlDecode(testCaseName);
            try
            {
                var testCase = _context.TestCases.FindAsync(decodedId).Result;
                var filePath = testCase.PayloadFilePath;

                Log.Information(
                    "GetMessagePayloadFile get file for protocol {Protocol}, testCaseName {TestCaseName} with filePath {FilePath}",
                    testCase.Protocol, testCase.TestName, filePath);

                return GetPayload(filePath);
            }
            catch(Exception e)
            {
                Log.Error(e,"GetMessagePayloadFile for protocol testCaseName {TestCaseName} failed", testCaseName);
                return new NotFoundResult();
            }
        }

       

        // GET api/<TestsCasePayloadFilesController>/TestCaseName/Attachement/attachmentFileName
        [HttpGet("{protocol}/{testCaseName}/Attachement/{attachmentFileName}")]
        public ActionResult GetAttachmentPayloadFile(string protocol, string testCaseName, string attachmentFileName)
        {
            var filePath = Path.Combine(TestsDirectoryPath, protocol, testCaseName, "Attachments", attachmentFileName);
            
            Log.Information("GetAttachmentPayloadFile get attachment for protocol {Protocol} testCaseName {TestCaseName}, attachmentFileName {AttachmentFileName} with filePath {FilePath}", protocol, testCaseName, filePath, attachmentFileName);

            return GetPayload(filePath);
        }

        private ActionResult GetPayload(string filePath)
        {
            try
            {
                var basepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return new FileContentResult(ReadAllBytes( basepath + "/" + filePath), "application/octet-stream");
            }
            catch (Exception exception)
            {
                Log.Error(exception, "GetPayload could not find file with filepath {FilePath}", filePath);
                return NotFound(exception.Message);
            }
        }
    }
}
