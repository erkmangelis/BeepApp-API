using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeepApp_API.Models;
using BeepApp_API.Data;
using Microsoft.AspNetCore.Authorization;

namespace BeepApp_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = ApiJwtTokens.ApiAuthScheme)]
    public class TestController : ControllerBase
    {
        private readonly BeepAppDbContext _context;

        public TestController(BeepAppDbContext context)
        {
            _context = context;
        }

        // GET: api/test
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Test>>> GetTests()
        {
            var tests = await _context.Tests.ToListAsync();
            return Ok(tests);
        }

        // GET: api/test/userId/{userId}
        [HttpGet("userId/{userId}")]
        public async Task<ActionResult<IEnumerable<Test>>> GetTestsByUserId(Guid userId)
        {
            // Kullanıcının takımlarını al
            var teamIds = await _context.Teams
                .Where(t => t.UserId == userId)
                .Select(t => t.Id)
                .ToListAsync();

            // Takımların oyuncularını al
            var playerIds = await _context.PlayerTeams
                .Where(pt => teamIds.Contains(pt.TeamId)) // TeamId'leri ile eşleşen oyuncular
                .Select(pt => pt.PlayerId)
                .ToListAsync();

            // Eşleşen PlayerId'li testleri al
            var tests = await _context.Tests
                .Where(t => playerIds.Contains(t.PlayerId))
                .ToListAsync();

            return Ok(tests);
        }

        // GET: api/test/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Test>> GetTestById(int id)
        {
            var test = await _context.Tests.FindAsync(id);

            if (test == null)
            {
                return NotFound();
            }

            return Ok(test);
        }

        // GET: api/test/playerId/{playerId}
        [HttpGet("playerId/{playerId}")]
        public async Task<ActionResult<IEnumerable<Test>>> GetTestsByPlayerId(int playerId)
        {
            var tests = await _context.Tests
                .Where(t => t.PlayerId == playerId) // Eşleşen PlayerId'li testleri al
                .ToListAsync();

            return Ok(tests);
        }

        // POST: api/test
        [HttpPost]
        public async Task<ActionResult<Test>> PostTest(Test test)
        {
            _context.Tests.Add(test);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTestById), new { id = test.Id }, test);
        }

        // DELETE: api/test/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTest(int id)
        {
            var test = await _context.Tests.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }

            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
