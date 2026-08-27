namespace MinimalEP.Infrastructure.Data.Interceptors;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using MinimalEP.Domain.Model;

public class AuditAndSoftDeleteInterceptor(IHttpContextAccessor httpContextAccessor)
  : SaveChangesInterceptor
{
  private Guid? CurrentUserId
  {
    get
    {
      var value = httpContextAccessor.HttpContext?.User?
          .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
      return Guid.TryParse(value, out var id) ? id : null;
    }
  }

  public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
  {
    UpdateAuditFields(eventData.Context);
    return base.SavingChanges(eventData, result);
  }

  public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
  {
    UpdateAuditFields(eventData.Context);
    return base.SavingChangesAsync(eventData, result, cancellationToken);
  }

  private void UpdateAuditFields(DbContext? context)
  {
    if (context is null)
      return;

    var now = DateTimeOffset.UtcNow;
    var currentUserId = CurrentUserId;

    foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
    {
      switch (entry.State)
      {
        case EntityState.Added:
          entry.Entity.Created = now;
          // Skriv inte över CreatedBy om det redan är satt explicit (t.ex. vid registrering)
          entry.Entity.CreatedBy ??= currentUserId;

          // Sätt EmployeeId automatiskt när en ny Workload skapas från den inloggade användaren
          if (entry.Entity is Workload workloadAdded && currentUserId.HasValue)
            workloadAdded.EmployeeId = currentUserId.Value;

          break;

        case EntityState.Modified:
          entry.Entity.Updated = now;
          entry.Entity.UpdatedBy = currentUserId;
          break;

        case EntityState.Deleted:
          entry.State = EntityState.Modified;
          entry.Entity.Deleted = now;
          entry.Entity.DeletedBy = currentUserId;
          break;
      }
    }
  }
}
