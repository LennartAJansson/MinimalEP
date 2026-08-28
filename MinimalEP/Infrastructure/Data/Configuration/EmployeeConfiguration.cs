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

    builder.Property(x => x.Email)
        .HasMaxLength(256)
        .IsRequired();

    builder.Property(x => x.GivenName)
        .HasMaxLength(100)
        .IsRequired();

    builder.Property(x => x.Surname)
        .HasMaxLength(100)
        .IsRequired();

    builder.Property(x => x.Age)
        .IsRequired();

    builder.Property(x => x.Position)
        .HasMaxLength(100)
        .IsRequired();

    builder.Property(x => x.PhoneNumber)
        .HasMaxLength(30)
        .IsRequired();

    // Owned type: Address columns are stored inline on the Employees table
    // (Address_Street, Address_PostalCode, Address_City by default EF convention).
    builder.OwnsOne(x => x.Address, address =>
    {
      address.Property(a => a.Street).HasMaxLength(200).IsRequired();
      address.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
      address.Property(a => a.City).HasMaxLength(100).IsRequired();
    });

    builder.Ignore(x => x.Name);

    builder.HasQueryFilter(x => x.Deleted == null);
  }
}
