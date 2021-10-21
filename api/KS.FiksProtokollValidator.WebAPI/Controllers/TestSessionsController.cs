using System;
using System.Collections.Generic;
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
            Log.Information("GetTestSession with id: {Id}", id);
            var testSession = await _context.TestSessions
                .Include(t => t.FiksRequests)
                .ThenInclude(r => r.FiksResponses)
                
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
                Log.Error("Session with id {Id} not found", id);
                return NotFound();
            }

            try
            {
                _fiksResponseValidator.Validate(testSession);
            }
            catch (Exception e)
            {
                Log.Error(e, "Validering av melding feilet");
                return BadRequest($"Validering av melding feilet: {e.Message}");
            }

            Log.Information("TestSession with id {Id} found", id);
            
            return testSession;
        }

        // POST: api/TestSessions
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TestSession>> PostTestSession([FromBody] TestRequest testRequest)
        {
            Log.Information("PostTestSession start");
            TestSession testSession = new TestSession();
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

            testSession.CreatedAt = DateTime.Now;
            
            testSession.FiksRequests = new List<FiksRequest>();

            foreach (var testId in testSession.SelectedTestCaseIds)
            {
                var fiksRequest = new FiksRequest
                {
                    TestCase = _context.TestCases.Find(testId)
                };

                try
                {
                    fiksRequest.MessageGuid = _fiksRequestMessageService.Send(fiksRequest, testSession.RecipientId);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Noe gikk galt ved sending av request til {FiksKonto}", testSession.RecipientId);
                    if (e.InnerException.Message.Contains("Ingen konto med id"))
                    {
                        Log.Error("TestSession FIKS-account {FiksKonto} is illegal", testSession.RecipientId);
                        return BadRequest("Ugyldig konto: " + testSession.RecipientId);
                    }
                    Log.Error("An Error occured when sending FIKS request with recipient ID {0}", testSession.RecipientId);
                    return StatusCode(500, e);
                }
                testSession.FiksRequests.Add(fiksRequest);
            }

            testSession.SelectedTestCaseIds.Clear();

            _context.TestSessions.Add(testSession);

            await _context.SaveChangesAsync();
            
            Log.Debug("Session successfully created with id {Id} and recipientId {RecipientId}", testSession.Id, testSession.RecipientId);

            return CreatedAtAction("GetTestSession", new {id = testSession.Id}, testSession);
        }
    }
}
