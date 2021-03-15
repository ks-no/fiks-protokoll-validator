using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using static System.IO.File;

namespace KS.FiksProtokollValidator.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCasePayloadFilesController : ControllerBase
    {
        private const string TestsDirectoryPath = @"TestCases/";

        // GET api/<TestsCasePayloadFilesController>/TestCaseName
        [HttpGet("{testCaseName}")]
        public ActionResult GetMessagePayloadFile(string testCaseName)
        {
            var filePath = Path.Combine(TestsDirectoryPath, testCaseName, "arkivmelding.xml");

            return GetPayload(filePath);
        }

        // GET api/<TestsCasePayloadFilesController>/TestCaseName/attachmentFileName
        [HttpGet("{testCaseName}/{attachmentFileName}")]
        public ActionResult GetAttachmentPayloadFile(string testCaseName, string attachmentFileName)
        {
            var filePath = Path.Combine(TestsDirectoryPath, testCaseName, "Attachments", attachmentFileName);

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
                return NotFound(exception.Message);
            }
        }
    }
}
