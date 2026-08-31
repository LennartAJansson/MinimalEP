namespace MinimalEP.Infrastructure.Data.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using MinimalEP.Domain.Model;

public class CustomerConfiguration 
  : IEntityTypeConfiguration<Customer>
{
  public void Configure(EntityTypeBuilder<Customer> builder)
  {
    builder.ToTable("Customers");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Name)
        .HasMaxLength(CustomerConstraints.NameMaxLength)
        .IsRequired();

    builder.Property(x => x.Email)
        .HasMaxLength(CustomerConstraints.EmailMaxLength)
        .IsRequired()
        .IsUnicode(false);

    builder.Property(x => x.RowVersion).IsRowVersion();

    builder.HasIndex(x => x.Email).IsUnique();

    builder.HasQueryFilter(x => x.Deleted == null);
  }
}
