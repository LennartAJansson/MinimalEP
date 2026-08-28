namespace MinimalEP.Infrastructure.Data.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using MinimalEP.Domain.Model;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
  public void Configure(EntityTypeBuilder<RefreshToken> builder)
  {
    builder.ToTable("RefreshTokens");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.TokenHash)
        .HasMaxLength(200)
        .IsRequired();

    builder.Property(x => x.ExpiresAt)
        .IsRequired();

    builder.HasIndex(x => x.UserId);
    builder.HasIndex(x => x.TokenHash).IsUnique();

    builder.HasQueryFilter(x => x.Deleted == null);
  }
}
