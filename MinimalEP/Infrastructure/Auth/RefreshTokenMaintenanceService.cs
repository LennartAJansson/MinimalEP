namespace MinimalEP.Infrastructure.Auth;

using System.Diagnostics;

using Microsoft.Extensions.Options;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Observability;

public sealed partial class RefreshTokenMaintenanceService(
  IServiceScopeFactory scopeFactory,
  IOptions<RefreshTokenMaintenanceOptions> options,
  TimeProvider timeProvider,
  RefreshTokenMaintenanceMetrics metrics,
  ILogger<RefreshTokenMaintenanceService> logger)
  : BackgroundService
{
  private readonly RefreshTokenMaintenanceOptions settings = options.Value;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var interval = TimeSpan.FromMinutes(settings.CleanupIntervalMinutes);
    using var timer = new PeriodicTimer(interval, timeProvider);

    while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
    {
      await RunCleanupAsync(stoppingToken);
    }
  }

  public async Task RunCleanupAsync(CancellationToken cancellationToken)
  {
    var now = timeProvider.GetUtcNow();
    var cutoff = now.AddDays(-settings.RetentionDays);

    using var activity = new Activity("refresh-token-maintenance.cleanup").Start();
    var startedAt = timeProvider.GetTimestamp();

    await using var scope = scopeFactory.CreateAsyncScope();
    var repository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

    var deleted = await repository.DeleteInactiveOlderThanAsync(cutoff, cancellationToken);
    var total = await repository.CountAsync(cancellationToken);
    var duration = timeProvider.GetElapsedTime(startedAt);

    metrics.ReportCleanup(duration, deleted, total);
    CleanupCompleted(logger, deleted, total, cutoff, duration.TotalMilliseconds);
  }

  [LoggerMessage(
    EventId = 5001,
    Level = LogLevel.Information,
    Message = "Refresh token cleanup completed. Deleted={DeletedCount}, Total={TotalCount}, Cutoff={CutoffUtc}, DurationMs={DurationMs}")]
  private static partial void CleanupCompleted(ILogger logger, int deletedCount, int totalCount, DateTimeOffset cutoffUtc, double durationMs);
}
