namespace MinimalEP.Infrastructure.Auth;

using System.ComponentModel.DataAnnotations;

public sealed class JwtOptions
{
  public const string SectionName = "Jwt";
  public const int MinimumDuration = 1;
  public const int MaximumAccessTokenMinutes = 1440;
  public const int MaximumRefreshTokenDays = 90;

  [Required, MinLength(32)]
  public string Key { get; init; } = string.Empty;

  [Required]
  public string Issuer { get; init; } = string.Empty;

  [Required]
  public string Audience { get; init; } = string.Empty;

  [Range(MinimumDuration, MaximumAccessTokenMinutes)]
  public int ExpiresInMinutes { get; init; } = 60;

  [Range(MinimumDuration, MaximumRefreshTokenDays)]
  public int RefreshTokenExpiresInDays { get; init; } = 7;
}
