namespace MinimalEP.Infrastructure.Observability;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using MinimalEP.Infrastructure.Data.Context;

public sealed class DatabaseHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
  public async Task<HealthCheckResult> CheckHealthAsync(
    HealthCheckContext context,
    CancellationToken cancellationToken = default)
  {
    await using var scope = scopeFactory.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    return await dbContext.Database.CanConnectAsync(cancellationToken)
      ? HealthCheckResult.Healthy()
      : HealthCheckResult.Unhealthy("The database is unavailable.");
  }
}
