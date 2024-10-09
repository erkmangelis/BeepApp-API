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
    public class PlayerController : ControllerBase
    {
        private readonly BeepAppDbContext _context;

        public PlayerController(BeepAppDbContext context)
        {
            _context = context;
        }

        // GET: api/Player
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            return await _context.Players.ToListAsync();
        }

        // GET: api/Player/id/5
        [HttpGet("id/{id}")]
        public async Task<ActionResult<Player>> GetPlayer(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }
            return Ok(player);
        }

        // GET: api/Player/teamId/{teamId}
        [HttpGet("teamId/{teamId}")]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayersByTeamId(int teamId)
        {
            // PlayerTeam tablosunda belirtilen teamId ile eşleşen kayıtları al
            var playerTeams = await _context.PlayerTeams
                .Where(pt => pt.TeamId == teamId)
                .ToListAsync();

            // Oyuncuların ID'lerini al
            var playerIds = playerTeams.Select(pt => pt.PlayerId).ToList();

            // Player tablosundan oyuncuları getir
            var players = await _context.Players
                .Where(p => playerIds.Contains(p.Id))
                .ToListAsync();

            if (!players.Any())
            {
                return NotFound("No players found for the specified team.");
            }

            return Ok(players);
        }


        // GET: api/Player/userId/{userId}
        [HttpGet("userId/{userId}")]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayersByUserId(Guid userId)
        {
            // İlk olarak, kullanıcının takımlarını al
            var team = await _context.Teams
                .Where(t => t.UserId == userId)
                .FirstOrDefaultAsync();

            if (team == null)
            {
                return NotFound("No team found for the user.");
            }

            // Takım ID'sini kullanarak ilgili oyuncuları al
            var playerTeams = await _context.PlayerTeams
                .Where(pt => pt.TeamId == team.Id)
                .ToListAsync();

            // Oyuncuların ID'lerini al
            var playerIds = playerTeams.Select(pt => pt.PlayerId).ToList();

            // Player tablosundan oyuncuları getir
            var players = await _context.Players
                .Where(p => playerIds.Contains(p.Id))
                .ToListAsync();

            return Ok(players);
        }


        // POST: api/Player
        [HttpPost]
        public async Task<ActionResult<Player>> SavePlayer(Player player)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID could not be determined.");
            }

            var team = await _context.Teams.FirstOrDefaultAsync(t => t.UserId == Guid.Parse(userId));
            if (team == null)
            {
                return BadRequest("No team found for the user.");
            }

            player.CreationDate = DateTime.UtcNow;
            player.UpdateDate = DateTime.UtcNow;

            _context.Players.Add(player);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("Could not save player.");
            }

            var playerTeam = new PlayerTeam
            {
                PlayerId = player.Id,
                TeamId = team.Id
            };
            _context.PlayerTeams.Add(playerTeam);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlayer", new { id = player.Id }, player);
        }

        // DELETE: api/Player/id/5
        [HttpDelete("id/{id}")]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }

            // PlayerTeam tablosundaki ilgili kaydı sil
            var playerTeam = await _context.PlayerTeams
                .Where(pt => pt.PlayerId == id)
                .ToListAsync();

            if (playerTeam.Any())
            {
                _context.PlayerTeams.RemoveRange(playerTeam);
            }

            // Oyuncuyu sil
            _context.Players.Remove(player);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
