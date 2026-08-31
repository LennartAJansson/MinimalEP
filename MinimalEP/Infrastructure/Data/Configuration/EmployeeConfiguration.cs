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
        .HasMaxLength(EmployeeConstraints.EmailMaxLength)
        .IsRequired();

    builder.Property(x => x.GivenName)
        .HasMaxLength(EmployeeConstraints.NameMaxLength)
        .IsRequired();

    builder.Property(x => x.Surname)
        .HasMaxLength(EmployeeConstraints.NameMaxLength)
        .IsRequired();

    builder.Property(x => x.Age)
        .IsRequired();

    builder.Property(x => x.Position)
        .HasMaxLength(EmployeeConstraints.PositionMaxLength)
        .IsRequired();

    builder.Property(x => x.PhoneNumber)
        .HasMaxLength(EmployeeConstraints.PhoneNumberMaxLength)
        .IsRequired();

    builder.Property(x => x.RowVersion).IsRowVersion();

    // Owned type: Address columns are stored inline on the Employees table
    // (Address_Street, Address_PostalCode, Address_City by default EF convention).
    builder.OwnsOne(x => x.Address, address =>
    {
      address.Property(a => a.Street).HasMaxLength(EmployeeConstraints.StreetMaxLength).IsRequired();
      address.Property(a => a.PostalCode).HasMaxLength(EmployeeConstraints.PostalCodeMaxLength).IsRequired();
      address.Property(a => a.City).HasMaxLength(EmployeeConstraints.CityMaxLength).IsRequired();
    });

    builder.Ignore(x => x.Name);

    builder.HasQueryFilter(x => x.Deleted == null);
  }
}
