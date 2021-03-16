using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KS.FiksProtokollValidator.WebAPI.Data;
using KS.FiksProtokollValidator.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;

namespace KS.FiksProtokollValidator.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors]
    [ApiController]
    public class TestCasesController : ControllerBase
    {
        private readonly FiksIOMessageDBContext _context;

        public TestCasesController(FiksIOMessageDBContext context)
        {
            _context = context;
        }

        // GET: api/TestCases
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestCase>>> GetTestCases()
        {
            return await _context.TestCases.ToListAsync();
        }

        // GET: api/TestCases/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestCase>> GetTestCase(int id)
        {
            var testCase = await _context.TestCases.FindAsync(id);

            if (testCase == null)
            {
                return NotFound();
            }

            return testCase;
        }

        // PUT: api/TestCases/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{testName}")]
        public async Task<IActionResult> PutTestCase(string testName, TestCase testCase)
        {
            if (!testName.Equals(testCase.TestName))
            {
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
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TestCases
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TestCase>> PostTestCase([FromBody] TestCase testCase)
        {
            _context.TestCases.Add(testCase);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTestCase", new { testName = testCase.TestName }, testCase);
        }

        private bool TestCaseExists(string testName)
        {
            return _context.TestCases.Any(e => e.TestName == testName);
        }
    }
}
