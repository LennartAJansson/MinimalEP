# Audit & Soft Delete — EF Core Interceptor

## Purpose
Centralized handling of audit fields and soft deletion via a `SaveChangesInterceptor`. No logic is scattered across handlers.

## BaseEntity
```csharp
public class BaseEntity
{
	public Guid Id { get; set; } = Guid.CreateVersion7();
	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
	public Guid? CreatedBy { get; set; }
	public Guid? UpdatedBy { get; set; }
	public Guid? DeletedBy { get; set; }
}
```

## Interceptor — core logic
```csharp
public class AuditAndSoftDeleteInterceptor(IHttpContextAccessor accessor) : SaveChangesInterceptor
{
	public override ValueTask<InterceptionResult<int>> SavingChangesAsync(...)
	{
		UpdateAuditFields(context.DbContext);
		return base.SavingChangesAsync(...);
	}

	private void UpdateAuditFields(DbContext context)
	{
		var userId = GetCurrentUserId();
		var now    = DateTimeOffset.UtcNow;

		foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
		{
			switch (entry.State)
			{
				case EntityState.Added:
					entry.Entity.Created   = now;
					entry.Entity.CreatedBy ??= userId;   // ??= respects an explicitly set value
					break;

				case EntityState.Modified:
					entry.Entity.Updated   = now;
					entry.Entity.UpdatedBy = userId;
					break;

				case EntityState.Deleted:                // Convert to soft delete
					entry.State            = EntityState.Modified;
					entry.Entity.Deleted   = now;
					entry.Entity.DeletedBy = userId;
					break;
			}
		}
	}
}
```

## EF Core — HasQueryFilter (mandatory)
Every `IEntityTypeConfiguration<T>` must include:
```csharp
builder.HasQueryFilter(x => x.Deleted == null);
```
Dapper queries filter manually with `WHERE Deleted IS NULL`.
With joins, soft-deleted related entities must also be filtered explicitly.

## DI registration — Singleton required
```csharp
services.AddSingleton<AuditAndSoftDeleteInterceptor>();
services.AddDbContextPool<ApplicationDbContext>((sp, options) =>
{
	var interceptor = sp.GetRequiredService<AuditAndSoftDeleteInterceptor>();
	options.UseSqlServer(connectionString)
			 .AddInterceptors(interceptor);
});
```
**Singleton required** — `AddDbContextPool` resolves the interceptor from the root provider. A scoped interceptor throws `InvalidOperationException`.
`IHttpContextAccessor` is thread-safe to hold in a singleton.

## Registration without a JWT (e.g. during registration)
During registration there is no JWT — the interceptor cannot resolve UserId from the `sub` claim.
Set `CreatedBy` explicitly **before** `SaveChanges`:
```csharp
employee.CreatedBy = userId;   // the interceptor uses ??= and will not overwrite it
```

## Concurrency boundary
`RowVersion` does not belong on `BaseEntity`, since not all tables are editable in the same way.
`Customer`, `Employee` and `Workload` each declare their own `byte[] RowVersion` and configure it with `.IsRowVersion()`.
The audit interceptor must not write or regenerate `RowVersion`; SQL Server owns the value.

On update, the client's last-read token is used as the EF Core original value. A stale write produces `DbUpdateConcurrencyException` and is mapped to `409 Conflict` in the respective slice.
