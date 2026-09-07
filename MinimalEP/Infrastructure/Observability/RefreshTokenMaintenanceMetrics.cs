namespace MinimalEP.Infrastructure.Observability;

using System.Diagnostics.Metrics;

public sealed class RefreshTokenMaintenanceMetrics : IDisposable
{
  public const string MeterName = "MinimalEP.RefreshTokenMaintenance";

  private readonly Meter meter = new(MeterName);
  private readonly Histogram<double> cleanupDurationMs;
  private readonly Counter<long> deletedTokens;
  private readonly ObservableGauge<long> totalTokensGauge;
  private long totalTokens;

  public RefreshTokenMaintenanceMetrics()
  {
    cleanupDurationMs = meter.CreateHistogram<double>("refresh_token_cleanup_duration_ms");
    deletedTokens = meter.CreateCounter<long>("refresh_token_cleanup_deleted_total");
    totalTokensGauge = meter.CreateObservableGauge("refresh_token_total", () => totalTokens);
  }

  public void ReportCleanup(TimeSpan duration, int deletedCount, int totalCount)
  {
    cleanupDurationMs.Record(duration.TotalMilliseconds);
    deletedTokens.Add(deletedCount);
    Interlocked.Exchange(ref totalTokens, totalCount);
  }

  public void Dispose()
  {
    meter.Dispose();
  }
}
