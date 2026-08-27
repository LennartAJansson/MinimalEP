namespace MinimalEP.Infrastructure.Data.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class ApplicationDbContextFactory
  : IDesignTimeDbContextFactory<ApplicationDbContext>
{
  public ApplicationDbContext CreateDbContext(string[] args)
  {
    // Hittar appsettings.json i ditt huvudprojekt
    var configuration = new ConfigurationBuilder()
      .AddUserSecrets<ApplicationDbContextFactory>()
      .Build();

    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    optionsBuilder.UseSqlServer(connectionString);

    return new ApplicationDbContext(optionsBuilder.Options);
  }
}