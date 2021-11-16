using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors]
    [ApiController]
    public class TestCasesController : ControllerBase
    {
        private readonly FiksIOMessageDBContext _context;
        
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        public TestCasesController(FiksIOMessageDBContext context)
        {
            _context = context;
        }

        // GET: api/TestCases
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestCase>>> GetTestCases()
        {
            Log.Information("Finding all TestCases");
            
            //Invalidate old cookie
            Response.Cookies.Delete("_testSessionId", new CookieOptions()
            {
                Secure = false,
            });
            
            var testSession = new TestSession()
            {
                Id = Guid.NewGuid(),
            };
            
            // Set testSessionId as a cookie
            var option = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(30),
                Path = "/",
                HttpOnly = false,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            };
            
            Response.Cookies.Append("_testSessionId", testSession.Id.ToString(), option);  
            
            await _context.TestSessions.AddAsync(testSession);
            await _context.SaveChangesAsync();
         
            return await _context.TestCases.ToListAsync();
        }
        
        // GET: api/TestCases/Protocol
        [HttpGet("Protocol/{protocol}")]
        public async Task<ActionResult<IEnumerable<TestCase>>> GetTestCases(string protocol)
        {
            Log.Information("Finding all TestCase for protocol {Protocol}", protocol);
            
            //Invalidate old cookie
            Response.Cookies.Delete("_testSessionId", new CookieOptions()
            {
                Secure = false,
            });
            
            var testSession = new TestSession()
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now
            };
            
            // Set testSessionId as a cookie
            var option = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(30),
                Path = "/",
                HttpOnly = false,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            };
            
            Response.Cookies.Append("_testSessionId", testSession.Id.ToString(), option);  
            
            await _context.TestSessions.AddAsync(testSession);
            await _context.SaveChangesAsync();
            return await _context.TestCases.Where(t => t.Protocol == protocol).ToListAsync();
        }

        // GET: api/TestCases/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestCase>> GetTestCase(int id)
        {
            Log.Information("Finding testcase with id: {Id}", id);
            var testCase = await _context.TestCases.FindAsync(id);

            if (testCase == null)
            {
                Log.Error("Could not find TestCase with id {Id}", id);
                return NotFound($"Could not find TestCase with id {id}");
            }
            
            Log.Debug("GetTestCase found TestCase with id {Id}", id);

            return testCase;
        }

        // PUT: api/TestCases/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{testName}")]
        public async Task<IActionResult> PutTestCase(string testName, TestCase testCase)
        {
            Log.Information("PutTestCase with {TestName} and {TestCaseName}", testName, testCase.TestName);
            if (!testName.Equals(testCase.TestName))
            {
                Log.Error("BadRequest for testName {TestName} and testCase.TestName {TestCaseTestName}", testName, testCase.TestName );
                return BadRequest();
            }

            _context.Entry(testCase).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TestCaseExists(testName))
                {
                    Log.Error("DbUpdateConcurrencyException occured. TestCase with testName {TestName} doesn't exist", testName);
                    return NotFound();
                }
                else
                {
                    Log.Error("DbUpdateConcurrencyException occured for request on testName {TestName}", testName);
                    throw;
                }
            }
            
            Log.Debug("PutTestCase saved successfully to DB");

            return NoContent();
        }
                
        // POST: api/TestCases
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TestCase>> PostTestCase([FromBody] TestCase testCase)
        {
            Log.Information("Posting testcases");
            await _context.TestCases.AddAsync(testCase);
            await _context.SaveChangesAsync();

            Log.Debug("PostTestCase saved successfully to DB");
            
            return CreatedAtAction("GetTestCase", new { testName = testCase.TestName }, testCase);
        }

        private bool TestCaseExists(string testName)
        {
            return _context.TestCases.Any(e => e.TestName == testName);
        }
    }
}
