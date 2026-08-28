namespace MinimalEP.Infrastructure.Auth;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using MinimalEP.Domain.Core;
using MinimalEP.Infrastructure.Data.Context;

// Idempotent role seeding — ensures the well-known roles exist before the app accepts requests.
// Run once at startup (see Program.cs) via a scope, since RoleManager is scoped.
public static class RoleSeeder
{
  // Applies any pending EF Core migrations, creating the database if it does not exist yet.
  // Safe to call on every startup — Migrate() is a no-op when the schema is already current.
  // This removes the need to manually run `dotnet ef database update` for local/demo scenarios.
  public static async Task MigrateDatabaseAsync(IServiceProvider serviceProvider)
  {
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
  }

  public static async Task SeedAsync(IServiceProvider serviceProvider)
  {
    using var scope = serviceProvider.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    foreach (var role in Roles.All)
    {
      if (!await roleManager.RoleExistsAsync(role))
      {
        await roleManager.CreateAsync(new IdentityRole<Guid>(role));
      }
    }
  }
}
