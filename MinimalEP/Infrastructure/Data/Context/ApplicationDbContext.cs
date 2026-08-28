namespace MinimalEP.Infrastructure.Data.Context;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using MinimalEP.Domain.Model;

//dotnet tool install --global dotnet-ef
//dotnet tool update --global dotnet-ef
//dotnet ef migrations add InitialCreate --project MinimalEP --startup-project MinimalEP --output-dir Infrastructure/Data/Migrations
//dotnet ef database update --project MinimalEP --startup-project MinimalEP
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
  : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
  public DbSet<Customer> Customers => Set<Customer>();
  public DbSet<Employee> Employees => Set<Employee>();
  public DbSet<Workload> Workloads => Set<Workload>();
  public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Hittar och applicerar alla IEntityTypeConfiguration automatiskt
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
  }
}
