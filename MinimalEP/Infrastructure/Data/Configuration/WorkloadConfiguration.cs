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
    builder.Property(x => x.Comments).HasMaxLength(WorkloadConstraints.CommentsMaxLength);
    builder.Property(x => x.RowVersion).IsRowVersion();

    builder.HasOne(x => x.Customer)
        .WithMany()
        .HasForeignKey(x => x.CustomerId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(x => x.Employee)
        .WithMany()
        .HasForeignKey(x => x.EmployeeId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(x => x.EmployeeId)
        .IsUnique()
        .HasFilter("[Stop] IS NULL AND [Deleted] IS NULL");

    builder.HasIndex(x => new { x.EmployeeId, x.Id })
        .HasFilter("[Deleted] IS NULL");

    builder.HasIndex(x => new { x.CustomerId, x.Id })
        .HasFilter("[Deleted] IS NULL");

    builder.HasQueryFilter(x => x.Deleted == null);
  }
}
