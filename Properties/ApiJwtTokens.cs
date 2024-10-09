using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeepApp_API
{
    public static class ApiJwtTokens
    {

        public const string Issuer = "Performanz";
        public const string Audience = "ApiUser";
        public const string SecretKey = "RVZIGQXgyfOagpWVOqLE5WXKjlkOpbmQq1x9lHhhQxmlmNVUWyi6y7QrWRwXkR3OkXCONGkicTTpvJ5Lk4T3bg78123123";

        public const string AuthSchemes = "Identity.Application" + "," + JwtBearerDefaults.AuthenticationScheme;

        public const string ApiAuthScheme = JwtBearerDefaults.AuthenticationScheme;
    }
}
