using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.Resources;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models;
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

        // GET api/<TestsCasePayloadFilesController>/Protocol/TestCaseId/payload
        [HttpGet("{testCaseId}/payload")]
        public ActionResult GetMessagePayloadFile(string testCaseId)
        {
            try
            {
                var testCase = _context.TestCases.FindAsync(testCaseId).Result;
                var payloadFileName = testCase.PayloadFileName != null ? testCase.PayloadFileName : PayloadNames.Dictionary[testCase.Protocol];

                if (testCase.SamplePath == null)
                {
                    return new NoContentResult();
                }
                var filePath = Path.Combine(testCase.SamplePath, payloadFileName);

                Log.Information(
                    "GetMessagePayloadFile get file for protocol {Protocol}, testCaseName {TestCaseName}, {TestCaseId} with filePath {FilePath}",
                    testCase.Protocol, testCase.TestName, testCaseId, filePath);
                
                var contentDispositionHeader = new ContentDisposition()
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
        
        // GET api/<TestsCasePayloadFilesController>/TestCaseId/payload
        [HttpGet("{testSessionId}/{testCaseId}/payload")]
        public ActionResult GetUsedMessagePayloadFile(string testSessionId, string testCaseId)
        {
            try
            {
                var testCase = _context.TestCases.FindAsync(testCaseId).Result;
                var testSession = _context.TestSessions
                    .Include(ts => ts.FiksRequests)
                    .ThenInclude(fr => fr.TestCase)
                    .Include(ts => ts.FiksRequests)
                    .ThenInclude(fr => fr.CustomPayloadFile).FirstOrDefaultAsync(ts => ts.Id == Guid.Parse(testSessionId)).Result;

                var fiksRequest = testSession.FiksRequests.FirstOrDefault(request => request.TestCase == testCase);

                if (fiksRequest == null)
                {
                    Log.Error("Fant ikke testcase med id {TestCaseId} for testsession med id {TestSessionId}", testCaseId, testSessionId);
                    return BadRequest($"Fant ikke testcase med id {testCaseId} for testsession med id {testSessionId}");
                }

                var contentDispositionHeader = new ContentDisposition()
                {
                    FileName = fiksRequest.CustomPayloadFile != null ? fiksRequest.CustomPayloadFile.Filename : testCase.PayloadFileName,
                    DispositionType = "attachment"
                };

                Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());

                if (fiksRequest.CustomPayloadFile != null)
                {
                    return new FileContentResult(fiksRequest.CustomPayloadFile.Payload, "application/octet-stream");
                }

                var filePath = Path.Combine(testCase.SamplePath, testCase.PayloadFileName !=null ? testCase.PayloadFileName : PayloadNames.Dictionary[testCase.Protocol]);

                Log.Information(
                    "GetMessagePayloadFile get file for protocol {Protocol}, testCaseName {TestCaseName}, {TestCaseId} with filePath {FilePath}",
                    testCase.Protocol, testCase.TestName, testCaseId, filePath);

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
        [HttpPost("{testcaseId}/payload")]
        public async Task<ActionResult> UploadCustomPayload(string testcaseId)
        {
            Log.Information("UploadCustomPayload start");
            // Get testSessionId from cookie
            var testSessionId = Request.Cookies["_testSessionId"];
            var newTestSession = false;
            var testSession = new TestSession();
            
            if (string.IsNullOrEmpty(testSessionId))
            {
                Log.Information("UploadCustomPayload could not find a testSessionId from cookie. Creating a new TestSession");
                testSession = new TestSession()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now
                };
                newTestSession = true;
            }
            else
            {
                testSession = _context.TestSessions.FirstAsync(s => s.Id == Guid.Parse(testSessionId)).Result ??
                    new TestSession()
                    {
                        Id = Guid.NewGuid(),
                    };
            }

            var file = Request.Form.Files[0];
                
            if(file.Length <= 0)
            {
                Log.Error("File size is zero");
                return BadRequest();
            }

            var stream = new MemoryStream();
            await file.CopyToAsync(stream);


            var fiksRequest = testSession.FiksRequests?.Find(fr => fr.TestCase.TestId.Equals(testcaseId));

            if (fiksRequest == null)
            {
                fiksRequest = new FiksRequest
                {
                    Id = Guid.NewGuid(),
                    TestCase = await _context.TestCases.FindAsync(testcaseId),
                    CustomPayloadFile = new FiksRequestPayload
                    {
                        Filename = file.FileName,
                        Payload = stream.ToArray()
                    }
                };
            }
            else
            {
                fiksRequest.CustomPayloadFile = new FiksRequestPayload
                {
                    Filename = file.FileName,
                    Payload = stream.ToArray()
                };
            }

            if (testSession.FiksRequests == null)
            {
                testSession.FiksRequests = new List<FiksRequest> {fiksRequest};
            }
            else
            {
                testSession.FiksRequests.Add(fiksRequest);
            }
            
            if (newTestSession)
            {
                await _context.TestSessions.AddAsync(testSession);    
            }

            await _context.FiksRequest.AddAsync(fiksRequest);
            
            await _context.SaveChangesAsync();
            
            Log.Information("UploadCustomPayload finished");
            
            return new OkResult();
        }
        
        // GET api/<TestsCasePayloadFilesController>/testCaseId/Attachement/attachmentFileName
        [HttpGet("{testCaseId}/Attachment/{attachmentFileName}")]
        public async Task<ActionResult> GetAttachmentPayloadFileAsync(string testCaseId, string attachmentFileName)
        {
            var testCase = await _context.TestCases.FindAsync(testCaseId); 
            
            var filePath = Path.Combine(testCase.SamplePath, "Attachments", attachmentFileName);
            
            Log.Information("GetAttachmentPayloadFile get attachment for protocol {Protocol} testCaseId {testCaseId}, attachmentFileName {AttachmentFileName} with filePath {FilePath}");

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
