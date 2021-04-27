using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using static System.IO.File;

namespace KS.FiksProtokollValidator.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCasePayloadFilesController : ControllerBase
    {
        private const string TestsDirectoryPath = @"TestCases/";
        
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        // GET api/<TestsCasePayloadFilesController>/TestCaseName
        [HttpGet("{testCaseName}")]
        public ActionResult GetMessagePayloadFile(string testCaseName)
        {
            var filePath = Path.Combine(TestsDirectoryPath, testCaseName, "arkivmelding.xml");

            Log.Debug("GetMessagePayloadFile get file for testCaseName {TestCaseName} with filePath {FilePath}",testCaseName, filePath);
            
            return GetPayload(filePath);
        }

        // GET api/<TestsCasePayloadFilesController>/TestCaseName/attachmentFileName
        [HttpGet("{testCaseName}/{attachmentFileName}")]
        public ActionResult GetAttachmentPayloadFile(string testCaseName, string attachmentFileName)
        {
            var filePath = Path.Combine(TestsDirectoryPath, testCaseName, "Attachments", attachmentFileName);
            
            Log.Debug("GetAttachmentPayloadFile get attachment for testCaseName {TestCaseName}, attachmentFileName {AttachmentFileName} with filePath {FilePath}",testCaseName, filePath, attachmentFileName);

            return GetPayload(filePath);
        }

        private ActionResult GetPayload(string filePath)
        {
            try
            {
                return new FileContentResult(ReadAllBytes(filePath), "application/octet-stream");
            }
            catch (Exception exception)
            {
                Log.Error("GetPayload could not find file with filepath {FilePath}", filePath);
                return NotFound(exception.Message);
            }
        }
    }
}
