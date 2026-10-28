using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace KS.FiksProtokollValidator.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors]
    [ApiController]
    public class TestCasesController : ControllerBase
    {
        private readonly FiksIOMessageDBContext _context;
        
        private static readonly ILogger Logger = Serilog.Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        public TestCasesController(FiksIOMessageDBContext context)
        {
            _context = context;
        }

        // GET: api/TestCases
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestCase>>> GetTestCases()
        {
            Logger.Information("Finding all TestCases");
            
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
            Logger.Information("Finding all TestCase for protocol {Protocol}", protocol);
            
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
            Logger.Information("Finding testcase with id: {Id}", id);
            var testCase = await _context.TestCases.FindAsync(id);

            if (testCase == null)
            {
                Logger.Error("Could not find TestCase with id {Id}", id);
                return NotFound($"Could not find TestCase with id {id}");
            }
            
            Logger.Debug("GetTestCase found TestCase with id {Id}", id);

            return testCase;
        }
    }
}
