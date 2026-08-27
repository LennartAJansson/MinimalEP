namespace MinimalEP.Infrastructure.Auth;

using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Data.Context;

public static class AuthExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddApplicationAuth(IConfiguration configuration)
    {
      // 1. Identity med Guid-nycklar, sparar i vår befintliga ApplicationDbContext
      services
        .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
          options.Password.RequireDigit = true;
          options.Password.RequiredLength = 8;
          options.Password.RequireNonAlphanumeric = false;
          options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

      // 2. JWT-autentisering
      var jwtSettings = configuration.GetSection("Jwt");
      var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

      services
        .AddAuthentication(options =>
        {
          options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
          options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
          };
        });

      services.AddAuthorization();

      // 3. Token-tjänst
      services.AddScoped<ITokenService, JwtTokenService>();

      return services;
    }
  }

  extension(IServiceCollection services)
  {
    // Registers the Bearer security scheme on all OpenAPI documents so Scalar shows the lock button.
    // Uses ConfigureAll to avoid calling AddOpenApi() again — that is already done
    // via AddApiVersioning().AddOpenApi() in Program.cs (suppresses AV0029).
    public IServiceCollection AddJwtOpenApi()
    {
      services.ConfigureAll<OpenApiOptions>(options =>
      {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
      });

      return services;
    }
  }
}
