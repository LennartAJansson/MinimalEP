namespace MinimalEP.Infrastructure.Data.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using MinimalEP.Infrastructure.Data;

public class ApplicationDbContextFactory
  : IDesignTimeDbContextFactory<ApplicationDbContext>
{
  public ApplicationDbContext CreateDbContext(string[] args)
  {
    var configuration = new ConfigurationBuilder()
      .AddUserSecrets<ApplicationDbContextFactory>()
      .Build();

    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    var connectionString = configuration.GetConnectionString(DatabaseOptions.ConnectionStringName);

    optionsBuilder.UseSqlServer(connectionString);

    return new ApplicationDbContext(optionsBuilder.Options);
  }
}
