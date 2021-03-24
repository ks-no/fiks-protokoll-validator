using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.FiksIO;
using KS.FiksProtokollValidator.WebAPI.Models;
using KS.FiksProtokollValidator.WebAPI.Validation;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                return NotFound();
            }

            _fiksResponseValidator.Validate(testSession);

            return testSession;
        }

        // POST: api/TestSessions
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TestSession>> PostTestSession([FromBody] TestSession testSession)
        {
            testSession.Id = Guid.NewGuid();

            testSession.CreatedAt = DateTime.Now;
            
            testSession.FiksRequests = new List<FiksRequest>();

            foreach (var testId in testSession.SelectedTestCaseIds)
            {
                var fiksRequest = new FiksRequest
                {
                    TestCase = _context.TestCases.Find(testId)
                };

                fiksRequest.MessageGuid = _fiksRequestMessageService.Send(fiksRequest, testSession.RecipientId);

                testSession.FiksRequests.Add(fiksRequest);
            }

            testSession.SelectedTestCaseIds.Clear();

            _context.TestSessions.Add(testSession);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTestSession", new {id = testSession.Id}, testSession);
        }
    }
}
