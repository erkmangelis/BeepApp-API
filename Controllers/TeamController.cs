using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeepApp_API.Models;
using BeepApp_API.Data;
using Microsoft.AspNetCore.Authorization;

namespace BeepApp_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = ApiJwtTokens.ApiAuthScheme)]

    public class TeamController : ControllerBase
    {
        private readonly BeepAppDbContext _context;

        public TeamController(BeepAppDbContext context)
        {
            _context = context;
        }

        // GET: api/Team
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
        {
            return await _context.Teams.ToListAsync();
        }

        // GET: api/Team/id/5
        [HttpGet("id/{id}")]
        public async Task<ActionResult<Team>> GetTeam(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            return Ok(team);
        }

        // GET: api/Team/userId/{userId}
        [HttpGet("userId/{userId}")]
        public async Task<ActionResult<IEnumerable<Team>>> GetTeamsByUserId(Guid userId)
        {
            var teams = await _context.Teams.Where(t => t.UserId == userId).ToListAsync();
            return Ok(teams);
        }

        // POST: api/Team
        [HttpPost]
        public async Task<ActionResult<Team>> SaveTeam(Team team)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID could not be determined.");
            }

            team.UserId = Guid.Parse(userId); // UserId'yi takım modeline atayın

            if (team.Id == 0)
            {
                _context.Teams.Add(team);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    if (TeamExists(team.Id))
                    {
                        return Conflict();
                    }
                    else
                    {
                        throw; // Hata durumunda detaylı bilgi için exception fırlatılıyor.
                    }
                }
                return CreatedAtAction("GetTeam", new { id = team.Id }, team);
            }
            else
            {
                var existingTeam = await _context.Teams.FindAsync(team.Id);
                if (existingTeam == null)
                {
                    return NotFound();
                }
                _context.Entry(existingTeam).CurrentValues.SetValues(team);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Hata durumunda detaylı bilgi için exception fırlatılıyor.
                    }
                }
                return NoContent();
            }
        }

        // DELETE: api/Team/id/5
        [HttpDelete("id/{id}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(t => t.Id == id);
        }
    }
}
