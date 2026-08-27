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

    // Id är Guid (UUID v7), lagras bäst som unikt index
    builder.HasKey(x => x.Id);

    builder.Property(x => x.Name)
        .HasMaxLength(100)
        .IsRequired();

    builder.Property(x => x.Email)
        .HasMaxLength(255)
        .IsRequired()
        .IsUnicode(false); // Optimerar för SQL Server (varchar istället för nvarchar)

    // Skapa ett unikt index på Email så vi inte får dubbletter
    builder.HasIndex(x => x.Email).IsUnique();

    // MAGI: Filtrera automatiskt bort alla mjukt raderade rader från ALLA queries!
    builder.HasQueryFilter(x => x.Deleted == null);
  }
}