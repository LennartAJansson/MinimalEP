namespace MinimalEP.Domain.Model;

public class RefreshToken : BaseEntity
{
  public required Guid UserId { get; set; }
  public required Guid FamilyId { get; set; }
  public required string TokenHash { get; set; }
  public required DateTimeOffset ExpiresAt { get; set; }
  public DateTimeOffset? RevokedAt { get; set; }
  public Guid? ReplacedByTokenId { get; set; }
  public byte[] RowVersion { get; set; } = [];

  public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
}
