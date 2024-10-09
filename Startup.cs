using BeepApp_API.Data;
using BeepApp_API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using BeepApp_API;

namespace BeepApp_API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
          policy =>
          {
              policy.AllowAnyOrigin()  // Allows any IP or domain
                    .AllowAnyHeader()  // Allows any headers
                    .AllowAnyMethod(); // Allows any HTTP methods (GET, POST, etc.)
          });
            });

            services.AddMemoryCache();

            services.Configure<SmtpSettings>(Configuration.GetSection("SmtpSettings"));
            services.AddTransient<IEmailService, EmailService>();

            // PostgreSQL ve Identity için gerekli servisleri ekliyoruz.
            services.AddDbContext<BeepAppDbContext>(options =>
               options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));


            services.AddIdentity<User, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<BeepAppDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication()
             .AddJwtBearer(config => {
                 config.TokenValidationParameters = new TokenValidationParameters()
                 {
                     ValidIssuer = ApiJwtTokens.Issuer,
                     ValidAudience = ApiJwtTokens.Audience,
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiJwtTokens.SecretKey)),
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                 };
             });
            services.AddControllers();

            // Swagger/OpenAPI yapılandırması
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BeepApp API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BeepApp API v1"));
            //}

            app.UseHttpsRedirection();

            app.UseRouting(); // UseRouting önce yer almalı

            app.UseCors("AllowAllOrigins");

            // Authentication middleware
            app.UseAuthentication();

            // Custom Middleware - UserProfileMiddleware'i ekliyoruz
            app.UseMiddleware<UserProfileMiddleware>();

            app.UseAuthorization();
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}
