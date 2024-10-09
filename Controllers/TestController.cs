using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeepApp_API.Models;
using BeepApp_API.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        // GET: api/test/userId
        [HttpGet("userId")]
        public async Task<ActionResult<IEnumerable<Test>>> GetTestsByUserProfile()
        {
            try
            {
                // HttpContext.Items üzerinden UserProfile'ı alıyoruz
                var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found.");
                }

                // Kullanıcıyı UserId ile buluyoruz
                var userProfile = await _context.Users.FindAsync(userId);
                if (userProfile == null)
                {
                    return Unauthorized("User not found.");
                }

                // Kullanıcının organizasyon bilgilerini kullanıyoruz
                var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == userProfile.OrganizationId);
                if (organization == null)
                {
                    return BadRequest("Organization not found.");
                }

                // Organizasyona ait oyuncuları al
                var playerIds = await _context.Players
                    .Where(p => p.OrganizationId == organization.Id && !p.IsDeleted)
                    .Select(p => p.Id)
                    .ToListAsync();

                // Bu oyunculara ait testleri al
                var tests = await _context.Tests
                    .Where(t => playerIds.Contains(t.PlayerId))
                    .ToListAsync();

                return Ok(tests);
            }
            catch (Exception ex)
            {
                // Loglama işlemi yapılabilir
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/test/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Test>> GetTestById(Guid id)
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
        public async Task<ActionResult<IEnumerable<Test>>> GetTestsByPlayerId(Guid playerId)
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
