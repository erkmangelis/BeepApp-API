using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeepApp_API.Models;
using System.Security.Claims;
using BeepApp_API.Data;
using BeepApp_API;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = ApiJwtTokens.ApiAuthScheme)]
public class TestController : ControllerBase
{
    private readonly BeepAppDbContext _context;

    public TestController(BeepAppDbContext context)
    {
        _context = context;
    }

    public class TestDto
    {
        public Guid PlayerId { get; set; }
        public double? Vo2Max { get; set; }
        public double? Speed { get; set; }
        public double? Distance { get; set; }
        public string? Score { get; set; }
        public TestMode TestMode { get; set; }
        public double? DefenceAvg { get; set; }
        public double? OffenceAvg { get; set; }
        public double? DefenceListLength { get; set; }
        public double? OffenceListLength { get; set; }
    }



    // GET: api/Test
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetTests()
    {
        var tests = await _context.Tests.ToListAsync();
        var doTests = await _context.DOTests.ToListAsync();

        return Ok(new { Tests = tests, DOTests = doTests });
    }

    // GET: api/Test/{testId}
    [HttpGet("{testId}")]
    public async Task<ActionResult<object>> GetTestById(Guid testId)
    {
        var test = await _context.Tests.FindAsync(testId);
        if (test != null)
        {
            return Ok(test);
        }

        var doTest = await _context.DOTests.FindAsync(testId);
        if (doTest != null)
        {
            return Ok(doTest);
        }

        return NotFound("Test not found.");
    }

    // GET: api/Test/player/{playerId}
    [HttpGet("player/{playerId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetTestsByPlayerId(Guid playerId)
    {
        var organizationId = User.FindFirstValue(ClaimTypes.NameIdentifier); // User'dan OrganizationId'yi al

        var tests = await _context.Tests
            .Where(t => t.PlayerId == playerId || t.OrganizationId.ToString() == organizationId)
            .ToListAsync();

        var doTests = await _context.DOTests
            .Where(d => d.PlayerId == playerId || d.OrganizationId.ToString() == organizationId)
            .ToListAsync();

        return Ok(new { Tests = tests, DOTests = doTests });
    }

    // POST: api/Test
    [HttpPost]
    public async Task<ActionResult> PostTest([FromBody] TestDto testDto)
    {
        if (testDto == null)
        {
            return BadRequest("Test data is null.");
        }

        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found.");
        }

        // Kullanıcının OrganizationId'sini al
        var userProfile = await _context.Users.FindAsync(userId);
        if (userProfile == null)
        {
            return Unauthorized("User not found.");
        }

        int organizationId = userProfile.OrganizationId; // OrganizationId'yi int olarak al

        if (testDto.TestMode == TestMode.DefenceOffenceTab) // TestMode 4
        {
            var doTest = new DOTest
            {
                PlayerId = testDto.PlayerId,
                OrganizationId = organizationId,
                DefenceAvg = testDto.DefenceAvg ?? default,
                OffenceAvg = testDto.OffenceAvg ?? default,
                DefenceListLength = testDto.DefenceListLength ?? default,
                OffenceListLength = testDto.OffenceListLength ?? default,
                CreatedAt = DateTime.UtcNow
            };

            _context.DOTests.Add(doTest);
        }
        else // Diğer durumlar için Tests tablosuna ekleyin
        {
            var test = new Test
            {
                PlayerId = testDto.PlayerId,
                OrganizationId = organizationId, // OrganizationId'yi int olarak kullan
                vo2Max = testDto.Vo2Max ?? default,
                Speed = testDto.Speed ?? default,
                Distance = testDto.Distance ?? default,
                Score = testDto.Score, // Zorunlu değil
                CreatedAt = DateTime.UtcNow,
                TestMode = testDto.TestMode
            };

            _context.Tests.Add(test);
        }

        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTests), new { id = testDto.PlayerId }, testDto);
    }


    // DELETE: api/Test/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTest(Guid id)
    {
        var test = await _context.Tests.FindAsync(id);
        if (test != null)
        {
            _context.Tests.Remove(test);
        }

        var doTest = await _context.DOTests.FindAsync(id);
        if (doTest != null)
        {
            _context.DOTests.Remove(doTest);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
