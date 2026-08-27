namespace MinimalEP.Infrastructure.Data.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using MinimalEP.Domain.Model;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
  public void Configure(EntityTypeBuilder<Employee> builder)
  {
    builder.ToTable("Employees");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Name)
        .HasMaxLength(100)
        .IsRequired();

    builder.Property(x => x.Age)
        .IsRequired();

    builder.Property(x => x.Position)
        .HasMaxLength(100)
        .IsRequired();

    builder.HasQueryFilter(x => x.Deleted == null);
  }
}
