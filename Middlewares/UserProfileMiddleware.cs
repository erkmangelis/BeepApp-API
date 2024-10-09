using BeepApp_API.Models;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class UserProfileMiddleware
{
    private readonly RequestDelegate _next;

    public UserProfileMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            // Scoped servisleri çözümlemek için HttpContext.RequestServices kullanıyoruz
            var userManager = context.RequestServices.GetRequiredService<UserManager<User>>();
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier); // Doğru claim'i alıyoruz
            var userProfile = await userManager.FindByIdAsync(userId);

            if (userProfile != null)
            {
                context.Items["userProfile"] = userProfile; // UserProfile'ı HttpContext.Items içerisine ekliyoruz
            }
        }

        await _next(context);
    }

}
