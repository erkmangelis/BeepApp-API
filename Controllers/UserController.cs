using BeepApp_API.Data;
using BeepApp_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeepApp_API.Controllers
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = ApiJwtTokens.ApiAuthScheme)]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly BeepAppDbContext _context;

        public UserController(BeepAppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userManager.Users
                .Where(u => !u.IsDeleted)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Name = u.Name,
                    Surname = u.Surname,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(string id)
        {
            var user = await _userManager.Users
                .Where(u => u.Id == id && !u.IsDeleted)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Name = u.Name,
                    Surname = u.Surname,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // GET: api/user/byUsername/{username}
        [HttpGet("byUsername/{username}")]
        public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
        {
            var user = await _userManager.Users
                .Where(u => u.UserName == username && !u.IsDeleted)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Name = u.Name,
                    Surname = u.Surname,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // GET: api/Player/userByOrganizationId/{orgId}
        [HttpGet("userByOrganizationId/{orgId}")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByOrganizationId(int orgId)
        {
            // Belirtilen OrgId ile eşleşen kullanıcıları sorguluyoruz
            var users = await _context.Users
                .Where(u => u.OrganizationId == orgId && !u.IsDeleted)  // Silinmemiş kullanıcıları getiriyoruz
                .ToListAsync();

            if (!users.Any())
            {
                return NotFound($"No users found for the organization with ID {orgId}.");
            }

            return Ok(users);
        }


        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserModel updatedUser)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.IsDeleted)
            {
                return NotFound();
            }

            // Kullanıcı bilgilerini güncelle
            user.UserName = updatedUser.Username;
            user.Email = updatedUser.Email;
            // Diğer güncellemeler burada yapılabilir

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.IsDeleted)
            {
                return NotFound();
            }

            // IsDeleted alanını true yapıyoruz
            user.IsDeleted = true;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }

    public class UpdateUserModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
    }

}
