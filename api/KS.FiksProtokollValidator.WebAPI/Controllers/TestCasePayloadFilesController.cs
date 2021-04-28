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
        [HttpGet("{protocol}/{testCaseName}/{fileName}")]
        public ActionResult GetMessagePayloadFile(string testCaseName, string protocol, string fileName)
        {
            var filePath = Path.Combine(TestsDirectoryPath, protocol, testCaseName, fileName);

            return GetPayload(filePath);
        }

        // GET api/<TestsCasePayloadFilesController>/TestCaseName/attachmentFileName
        [HttpGet("{protocol}/{testCaseName}/Attachement/{attachmentFileName}")]
        public ActionResult GetAttachmentPayloadFile(string protocol, string testCaseName, string attachmentFileName)
        {
            var filePath = Path.Combine(TestsDirectoryPath, protocol, testCaseName, "Attachments", attachmentFileName);

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
