namespace MinimalEP.Tests.Infrastructure;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using MinimalEP.Infrastructure.Data.Context;

public sealed class MinimalEpApplicationFactory : WebApplicationFactory<Program>
{
  private readonly string databaseName = $"MinimalEP_Test_{Guid.NewGuid():N}";

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder
      .UseEnvironment("Testing")
      .UseSetting("ConnectionStrings:DefaultConnection", $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True")
      .UseSetting("Jwt:Key", "integration-test-signing-key-at-least-32-characters")
      .UseSetting("Jwt:Issuer", "MinimalEP.Tests")
      .UseSetting("Jwt:Audience", "MinimalEP.Tests")
      .UseSetting("Jwt:ExpiresInMinutes", "5")
      .UseSetting("Jwt:RefreshTokenExpiresInDays", "1")
      .UseSetting("Cors:AllowedOrigins:0", "http://localhost:4200")
      .UseSetting("BootstrapAdmin:Enabled", "false")
      .UseSetting("Database:ApplyMigrationsOnStartup", "true");
    builder.ConfigureTestServices(services =>
    {
      services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
        options.DefaultForbidScheme = TestAuthHandler.SchemeName;
      }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
    });
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      try
      {
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureDeleted();
      }
      catch (InvalidOperationException)
      {
      }
    }

    base.Dispose(disposing);
  }
}
