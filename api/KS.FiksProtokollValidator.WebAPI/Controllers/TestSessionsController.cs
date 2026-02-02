using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.FiksIO;
using KS.FiksProtokollValidator.WebAPI.FiksIO.Connection;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors]
    [ApiController]
    public class TestSessionsController : ControllerBase
    {
        private readonly FiksIOMessageDBContext _context;
        private readonly ISendMessageService _sendMessageService;
        private readonly IFiksResponseValidator _fiksResponseValidator;
        
        private static readonly ILogger Logger = Serilog.Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        public TestSessionsController(FiksIOMessageDBContext context, ISendMessageService sendMessageService, IFiksResponseValidator fiksResponseValidator)
        {
            _context = context;
            _sendMessageService = sendMessageService;
            _fiksResponseValidator = fiksResponseValidator;
        }

        // GET: api/TestSessions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestSession>> GetTestSession(Guid id)
        {
            Logger.Information("GetTestSession with id: {SessionID}", id);
            var testSession = await _context.TestSessions
                .Include(t => t.FiksRequests)
                .ThenInclude(r => r.FiksResponses).ThenInclude(a => a.FiksPayloads)
                
                .Include(t => t.FiksRequests)
                .ThenInclude(r => r.TestCase)
                
                .Include(t => t.FiksRequests)
                .ThenInclude(r => r.CustomPayloadFile)

                .Include(t => t.FiksRequests)
                .ThenInclude(r => r.TestCase)
                .ThenInclude(a => a.FiksResponseTests)

                .Include(t => t.FiksRequests)
                .ThenInclude(r => r.TestCase)
                .ThenInclude(a => a.ExpectedResponseMessageTypes)

                .FirstOrDefaultAsync(i => i.Id == id);

            if (testSession == null)
            {
                Logger.Error("Session with id {SessionID} not found", id);
                return NotFound();
            }

            try
            {
                _fiksResponseValidator.Validate(testSession);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Validering av en eller flere meldinger feilet for session med id {SessionID}", id);
                return StatusCode(500, $"Validering av en eller flere meldinger feilet: {e.Message}");
            }

            Logger.Information("TestSession with id {Id} found", id);
            
            return testSession;
        }
        
        // POST: api/TestSessions
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TestSession>> PostTestSession([FromBody] TestRequest testRequest)
        {
            Logger.Information("PostTestSession start");
            
            TestSession testSession;
            var isNewTestSession = false;
            var testSessionId = Request.Cookies["_testSessionId"];
            
            //Trim recipient for leading and trailing white-spaces
            testRequest.RecipientId = testRequest.RecipientId.Trim();
            var selectedProtocol = testRequest.Protocol;
            
            if(!string.IsNullOrEmpty(testSessionId))
            {
                Logger.Debug("Finding session with sessionId {SessionId}", testRequest.SessionId);
                testSession = GetAndUpdateTestSession(testRequest, testSessionId);
            }
            else
            {
                Logger.Debug("Create new TestSession from incoming request");
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

                    Logger.Error("Error with deserializing the test request: {}", JsonSerializer.Serialize(testRequest));
                    return BadRequest(message);
                }
                testSession.Id = Guid.NewGuid();
                isNewTestSession = true;
            }
           
            testSession.CreatedAt = DateTime.Now;

            testSession.FiksRequests ??= new List<FiksRequest>();
            
            foreach (var testId in testSession.SelectedTestCaseIds)
            {
                var testCase = await _context.TestCases.FindAsync(testId);
                var isNewFiksRequest = false;
                var fiksRequest = testSession.FiksRequests.Find(fr => fr.TestCase == testCase);
                
                if(fiksRequest == null) {
                    fiksRequest = new FiksRequest
                    {
                        Id = Guid.NewGuid(),
                        TestCase = testCase
                    };
                    isNewFiksRequest = true;
                }

                try
                {
                    fiksRequest.MessageGuid = await _sendMessageService.Send(fiksRequest, testSession.RecipientId, selectedProtocol);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Noe gikk galt ved sending av request til {FiksKonto}", testSession.RecipientId);
                    
                    var errorMessage = e.Message;
                    if (e.InnerException != null)
                    {
                        errorMessage = e.InnerException.Message;
                    }
                    
                    if (errorMessage.Contains("Ingen konto med id") || errorMessage.Contains("NotFound"))
                    {
                        Logger.Error("TestSession FIKS-account {FiksKonto} was not found", testSession.RecipientId);
                        return BadRequest($"Ugyldig konto: {testSession.RecipientId}. Kontoen ble ikke funnet i FIKS-IO katalogen.");
                    }
                    
                    Logger.Error("An Error occured when sending FIKS request with recipient ID {RecipientId}", testSession.RecipientId);
                    return StatusCode(500, $"Feil ved sending til FIKS-IO: {errorMessage}");
                }

                if (isNewFiksRequest)
                {
                    testSession.FiksRequests.Add(fiksRequest);
                    await _context.FiksRequest.AddAsync(fiksRequest);
                }
                else
                {
                    _context.FiksRequest.Update(fiksRequest);
                }
            }

            testSession.SelectedTestCaseIds.Clear();

            if (isNewTestSession)
            {
                _context.TestSessions.Add(testSession);
            }
            else
            {
                _context.TestSessions.Update(testSession);
            }

            await _context.SaveChangesAsync();
            
            Logger.Debug("Session successfully created with id {Id} and recipientId {RecipientId}", testSession.Id, testSession.RecipientId);

            return CreatedAtAction("GetTestSession", new {id = testSession.Id}, testSession);
        }

        private TestSession GetAndUpdateTestSession(TestRequest testRequest, string testSessionId)
        {
            var testSession = _context.TestSessions
                .Include(t => t.FiksRequests)
                .ThenInclude(f => f.CustomPayloadFile)
                .FirstOrDefaultAsync(s => s.Id.Equals(Guid.Parse(testSessionId))).Result;

            var testSessionFromRequest = JsonSerializer.Deserialize<TestSession>(JsonSerializer.Serialize(testRequest));
            if (testSessionFromRequest != null)
            {
                testSession.RecipientId = testSessionFromRequest.RecipientId;
                testSession.SelectedTestCaseIds = testSessionFromRequest.SelectedTestCaseIds;
            }

            return testSession;
        }
    }
}
