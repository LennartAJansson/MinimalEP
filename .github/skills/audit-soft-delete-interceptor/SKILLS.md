# Audit & Soft Delete — EF Core Interceptor

## Syfte
Centraliserad hantering av audit-fält och mjuk radering via en `SaveChangesInterceptor`. Ingen logik sprids i handlers.

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

## Interceptor — kärnlogik
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
					entry.Entity.CreatedBy ??= userId;   // ??= respekterar explicit satt värde
					break;

				case EntityState.Modified:
					entry.Entity.Updated   = now;
					entry.Entity.UpdatedBy = userId;
					break;

				case EntityState.Deleted:                // Konvertera till mjuk radering
					entry.State            = EntityState.Modified;
					entry.Entity.Deleted   = now;
					entry.Entity.DeletedBy = userId;
					break;
			}
		}
	}
}
```

## EF Core — HasQueryFilter (obligatoriskt)
Varje `IEntityTypeConfiguration<T>` måste ha:
```csharp
builder.HasQueryFilter(x => x.Deleted == null);
```
Dapper-queries filtrerar manuellt med `WHERE Deleted IS NULL`.

## DI-registrering — Singleton krävs
```csharp
services.AddSingleton<AuditAndSoftDeleteInterceptor>();
services.AddDbContextPool<ApplicationDbContext>((sp, options) =>
{
	var interceptor = sp.GetRequiredService<AuditAndSoftDeleteInterceptor>();
	options.UseSqlServer(connectionString)
		   .AddInterceptors(interceptor);
});
```
**Singleton krävs** — `AddDbContextPool` löser interceptorn från root-provider. En scoped interceptor kastar `InvalidOperationException`.
`IHttpContextAccessor` är trådsäker att hålla i en singleton.

## Registrering utan JWT (t.ex. vid registrering)
Vid registrering finns inget JWT — interceptorn kan inte lösa UserId från `sub`-claim.
Sätt `CreatedBy` explicit **före** `SaveChanges`:
```csharp
employee.CreatedBy = userId;   // interceptorn använder ??= och skriver inte över
```
