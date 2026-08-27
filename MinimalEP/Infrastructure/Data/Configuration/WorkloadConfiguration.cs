namespace MinimalEP.Infrastructure.Data.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using MinimalEP.Domain.Model;

public class WorkloadConfiguration : IEntityTypeConfiguration<Workload>
{
  public void Configure(EntityTypeBuilder<Workload> builder)
  {
    builder.ToTable("Workloads");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Start).IsRequired();
    builder.Property(x => x.Stop);
    builder.Property(x => x.Comments).HasMaxLength(1000);

    builder.HasOne(x => x.Customer)
        .WithMany()
        .HasForeignKey(x => x.CustomerId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(x => x.Employee)
        .WithMany()
        .HasForeignKey(x => x.EmployeeId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasQueryFilter(x => x.Deleted == null);
  }
}
