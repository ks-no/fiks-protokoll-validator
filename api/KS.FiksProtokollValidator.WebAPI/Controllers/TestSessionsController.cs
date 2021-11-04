using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.FiksIO;
using KS.FiksProtokollValidator.WebAPI.Models;
using KS.FiksProtokollValidator.WebAPI.Validation;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors]
    [ApiController]
    public class TestSessionsController : ControllerBase
    {
        private readonly FiksIOMessageDBContext _context;
        private readonly IFiksRequestMessageService _fiksRequestMessageService;
        private readonly IFiksResponseValidator _fiksResponseValidator;
        
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        public TestSessionsController(FiksIOMessageDBContext context, IFiksRequestMessageService fiksRequestMessageService, IFiksResponseValidator fiksResponseValidator)
        {
            _context = context;
            _fiksRequestMessageService = fiksRequestMessageService;
            _fiksResponseValidator = fiksResponseValidator;
        }

        // GET: api/TestSessions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestSession>> GetTestSession(Guid id)
        {
            Log.Information("GetTestSession with id: {SessionID}", id);
            var testSession = await _context.TestSessions
                .Include(t => t.FiksRequests)
                .ThenInclude(r => r.FiksResponses).ThenInclude(a => a.FiksPayloads)
                
                .Include(t => t.FiksRequests)
                .ThenInclude(r => r.TestCase)

                .Include(t => t.FiksRequests)
                .ThenInclude(r => r.TestCase)
                .ThenInclude(a => a.FiksResponseTests)

                .Include(t => t.FiksRequests)
                .ThenInclude(r => r.TestCase)
                .ThenInclude(a => a.ExpectedResponseMessageTypes)

                .FirstOrDefaultAsync(i => i.Id == id);

            if (testSession == null)
            {
                Log.Error("Session with id {SessionID} not found", id);
                return NotFound();
            }

            try
            {
                _fiksResponseValidator.Validate(testSession);
            }
            catch (Exception e)
            {
                Log.Error(e, "Validering av en eller flere meldinger feilet for session med id {SessionID}", id);
                return StatusCode(500, $"Validering av en eller flere meldinger feilet: {e.Message}");
            }

            Log.Information("TestSession with id {Id} found", id);
            
            return testSession;
        }
        
        // POST: api/TestSessions/{sessionId}/testcases/{testcaseId}/payload
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost("{sessionId}/testcases/{testcaseId}/payload")]
        public async Task<ActionResult> UploadCustomPayload(string sessionId, string testcaseId)
        {
           
            var testSession = _context.TestSessions.FirstAsync(s => s.Id == Guid.Parse(sessionId)).Result ?? new TestSession()
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
                CustomPayloadFile = new FiksPayload()
                {
                    Filename = file.Name,
                    Payload = stream.ToArray()
                }
            };
            
            testSession.FiksRequests.Add(fiksRequest);
            
            await _context.TestSessions.AddAsync(testSession);
            
            return new OkResult();
        }

        // POST: api/TestSessions
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TestSession>> PostTestSession([FromBody] TestRequest testRequest)
        {
            Log.Information("PostTestSession start");
            TestSession testSession;
            var newTestSession = false;
            if(!string.IsNullOrEmpty(testRequest.SessionId))
            {
                Log.Debug("Finding session with sessionId {SessionId}", testRequest.SessionId);
                testSession = _context.TestSessions.FindAsync(testRequest.SessionId).Result;
            }
            else
            {
                Log.Debug("Create new TestSession from incoming request");
                try
                {
                    testSession = JsonSerializer.Deserialize<TestSession>(JsonSerializer.Serialize(testRequest));
                }
                catch (Exception e)
                {
                    var message = e.Message;
                    if (e.InnerException != null)
                    {
                        message = e.InnerException.Message;
                    }

                    Log.Error("Error with deserializing the test request: {}", JsonSerializer.Serialize(testRequest));
                    return BadRequest(message);
                }
                testSession.Id = Guid.NewGuid();
                newTestSession = true;
            }
           
            testSession.CreatedAt = DateTime.Now;
            
            testSession.FiksRequests = new List<FiksRequest>();

            foreach (var testId in testSession.SelectedTestCaseIds)
            {
                var testCase = await _context.TestCases.FindAsync(testId);
                var fiksRequest = testSession.FiksRequests.Find(fr => fr.TestCase == testCase) ?? new FiksRequest
                {
                    TestCase = testCase
                };

                try
                {
                    fiksRequest.MessageGuid = _fiksRequestMessageService.Send(fiksRequest, testSession.RecipientId);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Noe gikk galt ved sending av request til {FiksKonto}", testSession.RecipientId);
                    if (e.InnerException != null && e.InnerException.Message.Contains("Ingen konto med id"))
                    {
                        Log.Error("TestSession FIKS-account {FiksKonto} is illegal", testSession.RecipientId);
                        return BadRequest("Ugyldig konto: " + testSession.RecipientId);
                    }
                    Log.Error("An Error occured when sending FIKS request with recipient ID {RecipientId}", testSession.RecipientId);
                    return StatusCode(500, e);
                }
                testSession.FiksRequests.Add(fiksRequest);
            }

            testSession.SelectedTestCaseIds.Clear();

            if (newTestSession)
            {
                _context.TestSessions.Add(testSession);
            }

            await _context.SaveChangesAsync();
            
            Log.Debug("Session successfully created with id {Id} and recipientId {RecipientId}", testSession.Id, testSession.RecipientId);

            return CreatedAtAction("GetTestSession", new {id = testSession.Id}, testSession);
        }
    }
}
