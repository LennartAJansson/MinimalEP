namespace MinimalEP.Domain.Model;

using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
  public string? RefreshToken { get; set; }
  public DateTimeOffset? RefreshTokenExpiry { get; set; }
}
