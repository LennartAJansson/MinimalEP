namespace MinimalEP.Tests.Domain;

using MinimalEP.Domain.Model;

public sealed class RefreshTokenTests
{
  [Fact]
  public void IsActiveAt_returns_true_only_for_non_revoked_and_not_expired_tokens()
  {
    var now = DateTimeOffset.UtcNow;
    var active = new RefreshToken
    {
      UserId = Guid.CreateVersion7(),
      FamilyId = Guid.CreateVersion7(),
      TokenHash = "active",
      ExpiresAt = now.AddMinutes(5)
    };

    var revoked = new RefreshToken
    {
      UserId = Guid.CreateVersion7(),
      FamilyId = Guid.CreateVersion7(),
      TokenHash = "revoked",
      ExpiresAt = now.AddMinutes(5),
      RevokedAt = now.AddMinutes(-1)
    };

    var expired = new RefreshToken
    {
      UserId = Guid.CreateVersion7(),
      FamilyId = Guid.CreateVersion7(),
      TokenHash = "expired",
      ExpiresAt = now.AddMinutes(-1)
    };

    Assert.True(active.IsActiveAt(now));
    Assert.False(revoked.IsActiveAt(now));
    Assert.False(expired.IsActiveAt(now));
  }
}
