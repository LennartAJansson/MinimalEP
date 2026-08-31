namespace MinimalEP.Infrastructure.Auth;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using MinimalEP.Domain.Core;
using MinimalEP.Domain.Model;
using MinimalEP.Infrastructure.Data;
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
    var options = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    if (!options.ApplyMigrationsOnStartup)
      return;

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
        var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        if (!result.Succeeded)
          throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
      }
    }

    var options = scope.ServiceProvider.GetRequiredService<IOptions<BootstrapAdminOptions>>().Value;
    if (!options.Enabled)
      return;

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    if (await userManager.FindByEmailAsync(options.Email) is not null)
      return;

    if (await userManager.Users.AnyAsync())
      throw new InvalidOperationException("BootstrapAdmin can only create an account in an empty installation.");

    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await using var transaction = await context.Database.BeginTransactionAsync();
    var userId = Guid.CreateVersion7();
    var user = new ApplicationUser
    {
      Id = userId,
      UserName = options.Email,
      Email = options.Email
    };

    var createResult = await userManager.CreateAsync(user, options.Password);
    if (!createResult.Succeeded)
      throw new InvalidOperationException(string.Join(", ", createResult.Errors.Select(e => e.Description)));

    var roleResult = await userManager.AddToRolesAsync(user, Roles.All);
    if (!roleResult.Succeeded)
      throw new InvalidOperationException(string.Join(", ", roleResult.Errors.Select(e => e.Description)));

    context.Employees.Add(new Employee
    {
      Id = userId,
      Email = options.Email,
      GivenName = options.GivenName,
      Surname = options.Surname,
      Age = options.Age,
      Position = options.Position,
      PhoneNumber = options.PhoneNumber,
      Address = new Address
      {
        Street = options.Street,
        PostalCode = options.PostalCode,
        City = options.City
      },
      CreatedBy = userId
    });
    await context.SaveChangesAsync();
    await transaction.CommitAsync();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(RoleSeeder));
    logger.LogInformation("Bootstrap SuperAdmin {UserId} was created.", userId);
  }
}
