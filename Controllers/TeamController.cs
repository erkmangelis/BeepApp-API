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
            // IsDeleted false olan takımları döndür
            return await _context.Teams
                .Where(t => !t.IsDeleted)
                .ToListAsync();
        }

        // GET: api/Team/id/{id}
        [HttpGet("id/{id}")]
        public async Task<ActionResult<Team>> GetTeam(Guid id)
        {
            var team = await _context.Teams
                .Where(t => !t.IsDeleted && t.Id == id)
                .FirstOrDefaultAsync();

            if (team == null)
            {
                return NotFound();
            }
            return Ok(team);
        }

        // GET: api/Team/organizationId/{organizationId}
        [HttpGet("organizationId/{organizationId}")]
        public async Task<ActionResult<IEnumerable<Team>>> GetTeamsByOrganizationId(int organizationId)
        {
            // OrganizationId ile eşleşen silinmemiş takımları döndür
            var teams = await _context.Teams
                .Where(t => t.OrganizationId == organizationId && !t.IsDeleted)
                .ToListAsync();

            if (!teams.Any())
            {
                return NotFound($"No teams found for the organization with ID {organizationId}.");
            }

            return Ok(teams);
        }

        // POST: api/Team
        [HttpPost]
        public async Task<ActionResult<Team>> SaveTeam(Team team)
        {
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

            team.OrganizationId = userProfile.OrganizationId; // OrganizationId'yi kullanıcıya göre ayarla
            team.CreatedAt = DateTime.UtcNow;
            team.UpdatedAt = DateTime.UtcNow;

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTeam", new { id = team.Id }, team);
        }

        // PUT: api/Team/id/{id}
        [HttpPut("id/{id}")]
        public async Task<IActionResult> UpdateTeam(Guid id, Team updatedTeam)
        {
            if (id != updatedTeam.Id)
            {
                return BadRequest("Team ID mismatch.");
            }

            var existingTeam = await _context.Teams.FindAsync(id);
            if (existingTeam == null || existingTeam.IsDeleted)
            {
                return NotFound("Team not found.");
            }

            existingTeam.Name = updatedTeam.Name;
            existingTeam.UpdatedAt = DateTime.UtcNow;
            _context.Entry(existingTeam).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Team/id/{id}
        [HttpDelete("id/{id}")]
        public async Task<IActionResult> DeleteTeam(Guid id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null || team.IsDeleted)
            {
                return NotFound("Team not found.");
            }

            // Silme yerine IsDeleted'i true yapıyoruz
            team.IsDeleted = true;
            _context.Entry(team).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TeamExists(Guid id)
        {
            return _context.Teams.Any(t => t.Id == id && !t.IsDeleted);
        }
    }
}
