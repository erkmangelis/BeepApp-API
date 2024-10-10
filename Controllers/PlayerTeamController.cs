using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeepApp_API.Data;
using BeepApp_API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeepApp_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerTeamController : ControllerBase
    {
        private readonly BeepAppDbContext _context;

        public PlayerTeamController(BeepAppDbContext context)
        {
            _context = context;
        }

        // GET: api/PlayerTeam
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTeamsWithPlayers()
        {
            var playerTeams = await _context.PlayerTeams
                .ToListAsync();

            // Grouping by TeamId to get players in each team
            var teamsWithPlayers = playerTeams
                .GroupBy(pt => pt.TeamId)
                .Select(group => new
                {
                    TeamId = group.Key,
                    Players = group.Select(pt => new
                    {
                        pt.PlayerId
                    }).ToList()
                })
                .ToList();

            return Ok(teamsWithPlayers);
        }

        // GET: api/PlayerTeam/{teamId}
        [HttpGet("{teamId}")]
        public async Task<ActionResult<object>> GetTeamWithPlayers(Guid teamId)
        {
            var playerTeams = await _context.PlayerTeams
                .Where(pt => pt.TeamId == teamId)
                .ToListAsync();

            if (!playerTeams.Any())
            {
                return NotFound("Team not found or has no players.");
            }

            var teamWithPlayers = new
            {
                TeamId = teamId,
                Players = playerTeams.Select(pt => new
                {
                    pt.PlayerId
                }).ToList()
            };

            return Ok(teamWithPlayers);
        }

        // POST: api/PlayerTeam
        [HttpPost]
        public async Task<ActionResult<PlayerTeam>> CreatePlayerTeam([FromBody] PlayerTeam playerTeam)
        {
            if (playerTeam == null)
            {
                return BadRequest("Invalid player-team data.");
            }

            _context.PlayerTeams.Add(playerTeam);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeamWithPlayers), new { teamId = playerTeam.TeamId }, playerTeam);
        }

        // DELETE: api/PlayerTeam/{teamId}/{playerId}
        [HttpDelete("{teamId}/{playerId}")]
        public async Task<IActionResult> DeletePlayerTeam(Guid teamId, Guid playerId)
        {
            var playerTeam = await _context.PlayerTeams
                .FirstOrDefaultAsync(pt => pt.TeamId == teamId && pt.PlayerId == playerId);

            if (playerTeam == null)
            {
                return NotFound("Player-Team relationship not found.");
            }

            _context.PlayerTeams.Remove(playerTeam);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
