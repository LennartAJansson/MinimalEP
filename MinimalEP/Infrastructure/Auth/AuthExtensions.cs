namespace MinimalEP.Infrastructure.Auth;

using System.Text;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using MinimalEP.Domain.Core;
using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Data.Context;

public static class AuthExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddApplicationAuth(IConfiguration configuration)
    {
      services.AddOptions<JwtOptions>()
        .Bind(configuration.GetRequiredSection(JwtOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

      services.AddSingleton<IValidateOptions<BootstrapAdminOptions>, BootstrapAdminOptionsValidator>();
      services.AddOptions<BootstrapAdminOptions>()
        .Bind(configuration.GetRequiredSection(BootstrapAdminOptions.SectionName))
        .ValidateOnStart();

      services
        .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
          options.Password.RequireDigit = true;
          options.Password.RequiredLength = AuthDefaults.PasswordMinimumLength;
          options.Password.RequireNonAlphanumeric = false;
          options.User.RequireUniqueEmail = true;
          options.Lockout.AllowedForNewUsers = true;
          options.Lockout.MaxFailedAccessAttempts = AuthDefaults.MaxFailedAccessAttempts;
          options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(AuthDefaults.LockoutMinutes);
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

      var jwtSettings = configuration.GetRequiredSection(JwtOptions.SectionName).Get<JwtOptions>()!;
      var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

      services
        .AddAuthentication(options =>
        {
          options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
          options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
          // Keep claim types as issued (e.g. "sub") instead of mapping them to legacy
          // WS-Federation claim URIs (e.g. ClaimTypes.NameIdentifier). Must match the
          // MapInboundClaims setting used everywhere else tokens are read (see
          // RefreshTokenHandler and .github/Skills/identity-jwt-auth).
          options.MapInboundClaims = false;
          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
          };
        });

      services.AddAuthorizationBuilder()
        .AddPolicy(AuthorizationPolicies.SuperAdminOnly, p => p.RequireRole(Roles.SuperAdmin))
        .AddPolicy(AuthorizationPolicies.AdminOrAbove, p => p.RequireRole(Roles.SuperAdmin, Roles.Admin));

      services.AddRateLimiter(options =>
      {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy(RateLimitPolicies.Authentication, httpContext =>
          RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
              PermitLimit = AuthDefaults.RateLimitPermitCount,
              Window = TimeSpan.FromMinutes(AuthDefaults.RateLimitWindowMinutes),
              QueueLimit = 0,
              AutoReplenishment = true
            }));
      });

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
