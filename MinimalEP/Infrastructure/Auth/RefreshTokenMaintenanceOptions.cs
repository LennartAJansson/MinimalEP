namespace MinimalEP.Infrastructure.Auth;

using System.ComponentModel.DataAnnotations;

public sealed class RefreshTokenMaintenanceOptions
{
  public const string SectionName = "RefreshTokenMaintenance";

  [Range(1, 3650)]
  public int RetentionDays { get; init; } = 30;

  [Range(1, 1440)]
  public int CleanupIntervalMinutes { get; init; } = 60;
}
