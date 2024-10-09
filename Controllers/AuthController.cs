using BeepApp_API.Data;
using BeepApp_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BeepApp_API;

namespace BeepApp_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly BeepAppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, BeepAppDbContext context, IMemoryCache cache, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _cache = cache;
            _emailService = emailService;
        }
        public class RegisterModel
        {
            public string Email { get; set; }

            public string Password { get; set; }

            public string Name { get; set; }

            public string Username { get; set; }

            public string Surname { get; set; }

            public string ConfirmPassword { get; set; }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                Name = model.Name,
                Surname = model.Surname,

            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            return Ok("User registered successfully!");
        }


        public class LoginModel
        {
            public string UserNameOrEmail { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kullanıcıyı e-posta veya kullanıcı adına göre bulmaya çalışıyoruz.
            var user = await _userManager.FindByEmailAsync(model.UserNameOrEmail) ??
                       await _userManager.FindByNameAsync(model.UserNameOrEmail);

            if (user == null)
                return Unauthorized("Invalid login attempt.");

            var result = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!result)
                return Unauthorized("Invalid login attempt.");

            // Kullanıcı başarılı şekilde giriş yaptı, şimdi token oluşturuyoruz.
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                user.Id,
                Name = user.Name,
                user.Surname,
                user.UserName,
                Token = token,
            });
        }



        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("name", user.Name + " " + user.Surname),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiJwtTokens.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: ApiJwtTokens.Issuer,
                audience: ApiJwtTokens.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public class ChangePasswordModel
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            public string UserId { get; set; }

        }

        public class ForgotPasswordModel
        {
            public string Email { get; set; }
            public string UserName { get; set; }
        }

        public class ResetPasswordModel
        {
            public string Email { get; set; }
            public string Code { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmPassword { get; set; }
        }


        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
                return Unauthorized("Kullanıcı bulunamadı.");



            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            return Ok("Şifreniz başarıyla değiştirildi.");
        }


        [HttpPost("SendResetCode")]

        public async Task<IActionResult> SendResetCode([FromBody] ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email) ?? await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                return BadRequest("Kullanıcı bulunamadı.");

            // 6 haneli rastgele bir doğrulama kodu oluşturma
            var code = new Random().Next(100000, 999999).ToString();

            // Kodu cache'e 15 dakika süreyle sakla
            _cache.Set(user.Id, code, TimeSpan.FromMinutes(15));

            // E-posta gönderme işlemi
            var message = $"Şifre sıfırlama kodunuz: {code}";
            await _emailService.SendEmailAsync(user.Email, "Şifre Sıfırlama Kodu", message);

            return Ok("Şifre sıfırlama kodu e-posta adresinize gönderildi.");
        }

        [HttpPost("VerifyResetCode")]
        public async Task<IActionResult> VerifyResetCode([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Kullanıcı bulunamadı.");

            // Cache'de saklanan kodu kontrol et
            if (_cache.TryGetValue(user.Id, out string cachedCode))
            {
                if (cachedCode == model.Code)
                {
                    // Kod doğruysa, şifreyi sıfırla
                    var result = await _userManager.ResetPasswordAsync(user, await _userManager.GeneratePasswordResetTokenAsync(user), model.NewPassword);
                    if (result.Succeeded)
                    {
                        // Başarılı olursa kodu cache'den sil
                        _cache.Remove(user.Id);
                        return Ok("Şifreniz başarıyla sıfırlandı.");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }
                return BadRequest("Geçersiz kod.");
            }
            return BadRequest("Kodun süresi dolmuş.");
        }


    }
}
