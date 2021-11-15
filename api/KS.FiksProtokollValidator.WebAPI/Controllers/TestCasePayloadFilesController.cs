using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET api/<TestsCasePayloadFilesController>/TestCaseId/payload
        [HttpGet("{protocol}/{testCaseId}/payload")]
        public ActionResult GetMessagePayloadFile(string testCaseId)
        {
            try
            {
                var testCase = _context.TestCases.FindAsync(testCaseId).Result;
                var filePath = testCase.PayloadFilePath;

                Log.Information(
                    "GetMessagePayloadFile get file for protocol {Protocol}, testCaseName {TestCaseName}, {TestCaseId} with filePath {FilePath}",
                    testCase.Protocol, testCase.TestName, testCaseId, filePath);
                
                var contentDispositionHeader = new System.Net.Mime.ContentDisposition()
                {
                    FileName = testCase.PayloadFileName,
                    DispositionType = "attachment"
                };
                
                Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());
                
                return GetPayload(filePath);
            }
            catch(Exception e)
            {
                Log.Error(e,"GetMessagePayloadFile for protocol testCaseName {TestCaseName} failed", testCaseId);
                return new NotFoundResult();
            }
        }
        
        // POST: api/TestSessions/{sessionId}/testcases/{testcaseId}/payload
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost("{protocol}/{testcaseId}/payload")]
        public async Task<ActionResult> UploadCustomPayload(string testSessionId, string testcaseId)
        {
            // Get testSessionId from cookie
            testSessionId =  Request.Cookies["testSessionId"];
            var testSession = _context.TestSessions.FirstAsync(s => s.Id == Guid.Parse(testSessionId)).Result ?? new TestSession()
            {
                Id = new Guid(),
            };

            var file = Request.Form.Files[0];
                
            if(file.Length <= 0)
            {
                Log.Error("File size is zero");
                return BadRequest();
            }

            var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            
            var fiksRequest = new FiksRequest
            {
                TestCase = await _context.TestCases.FindAsync(testcaseId),
                CustomPayloadFile = new FiksRequestPayload()
                {
                    Filename = file.Name,
                    Payload = stream.ToArray()
                }
            };
            
            testSession.FiksRequests.Add(fiksRequest);
            
            await _context.TestSessions.AddAsync(testSession);
            await _context.SaveChangesAsync();
            
            return new OkResult();
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
