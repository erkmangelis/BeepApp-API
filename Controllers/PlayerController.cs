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
using Microsoft.AspNetCore.Identity;

namespace BeepApp_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = ApiJwtTokens.ApiAuthScheme)]
    public class PlayerController : ControllerBase
    {
        private readonly BeepAppDbContext _context;
        private readonly UserManager<User> _userManager; // UserManager bağımlılığı eklendi

        public PlayerController(BeepAppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager; // UserManager bağımlılığı atandı
        }

        // GET: api/Player
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            // Silinmiş oyuncuları hariç tutarak sorgulama
            var players = await _context.Players
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            return Ok(players);
        }

        // GET: api/Player/id/{id}
        [HttpGet("id/{id}")]
        public async Task<ActionResult<Player>> GetPlayer(Guid id)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (player == null)
            {
                return NotFound();
            }

            return Ok(player);
        }

        // GET: api/Player/teamId/{teamId}
        [HttpGet("teamId/{teamId}")]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayersByTeamId(Guid teamId)
        {
            // PlayerTeam tablosunda belirtilen teamId ile eşleşen kayıtları al
            var playerTeams = await _context.PlayerTeams
                .Where(pt => pt.TeamId == teamId)
                .ToListAsync();

            // Oyuncuların ID'lerini al
            var playerIds = playerTeams.Select(pt => pt.PlayerId).ToList();

            // Player tablosundan oyuncuları getir
            var players = await _context.Players
                .Where(p => playerIds.Contains(p.Id) && !p.IsDeleted)
                .ToListAsync();

            if (!players.Any())
            {
                return NotFound("No players found for the specified team.");
            }

            return Ok(players);
        }

        // GET: api/Player/organization
        [HttpGet("organization")]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayersByOrganization()
        {
            // HttpContext.Items üzerinden UserProfile'ı alıyoruz
            var userProfile = HttpContext.Items["userProfile"] as User; // User modelini kullanıyoruz
            if (userProfile == null)
            {
                return Unauthorized("UserProfile is null.");
            }

            // Kullanıcının organizasyon ID'sini alıyoruz
            var organizationId = userProfile.OrganizationId;

            // Organizasyona ait tüm oyuncuları getiriyoruz ve silinmiş oyuncuları dahil etmiyoruz
            var players = await _context.Players
                .Where(p => p.OrganizationId == organizationId && !p.IsDeleted)
                .ToListAsync();

            if (!players.Any())
            {
                return NotFound("No players found for the organization.");
            }

            return Ok(players);
        }



        // POST: api/Player
        [HttpPost]
        public async Task<ActionResult<Player>> SavePlayer([FromBody] Player player)
        {
            if (player == null)
            {
                return BadRequest("Player is null.");
            }

            try
            {
                // HttpContext.Items üzerinden UserProfile'ı alıyoruz
                var userProfile = HttpContext.Items["userProfile"] as User; // User modelini burada kullanıyoruz
                if (userProfile == null)
                {
                    return Unauthorized("UserProfile is null.");
                }

                // Kullanıcının organizasyon bilgilerini kullanıyoruz
                var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == userProfile.OrganizationId);
                if (organization == null)
                {
                    return BadRequest("Organization not found.");
                }

                player.OrganizationId = organization.Id;
                player.AddedBy = Guid.Parse(userProfile.Id);
                player.CreatedAt = DateTime.UtcNow;
                player.UpdatedAt = DateTime.UtcNow;
                player.IsDeleted = false;

                _context.Players.Add(player);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
            }
            catch (Exception ex)
            {
                // Loglama işlemi yapılabilir
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Player/id/{id}
        [HttpDelete("id/{id}")]
        public async Task<IActionResult> DeletePlayer(Guid id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }

            // Oyuncunun IsDeleted alanını true yap
            player.IsDeleted = true;
            _context.Players.Update(player);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
