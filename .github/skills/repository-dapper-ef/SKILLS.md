# Repository Pattern with EF Core + Dapper

## Purpose
Separation of Concerns: Dapper for reads (fast, no overhead), EF Core for writes (the interceptor requires the change tracker).

## Interface (Features/Core/)
```csharp
public interface ICustomerRepository
{
	Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct, bool tracked = false);
	Task<PagedResult<Customer>> GetPageAsync(PageRequest page, CancellationToken ct);
	Task AddAsync(Customer entity, CancellationToken ct);
	void Remove(Customer entity);
	void SetOriginalRowVersion(Customer entity, byte[] rowVersion);
	Task SaveChangesAsync(CancellationToken ct);
}
```

## Implementation (Infrastructure/Data/Core/)
```csharp
public class CustomerRepository(ApplicationDbContext context, IDbConnectionFactory connectionFactory)
	: ICustomerRepository
{
	public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct, bool tracked = false)
	{
		if (tracked)
			return await context.Customers
				.FirstOrDefaultAsync(x => x.Id == id && x.Deleted == null, ct);

		using IDbConnection db = connectionFactory.CreateConnection();
		return await db.QuerySingleOrDefaultAsync<Customer>(new CommandDefinition(
			"SELECT Id, Name, Email, RowVersion FROM Customers WHERE Id = @Id AND Deleted IS NULL",
			new { Id = id }, cancellationToken: ct));
	}

	public async Task<PagedResult<Customer>> GetPageAsync(PageRequest page, CancellationToken ct)
	{
		using IDbConnection db = connectionFactory.CreateConnection();
		var result = await db.QueryAsync<Customer>(new CommandDefinition(
			"SELECT TOP (@Take) Id, Name, Email, RowVersion FROM Customers WHERE Deleted IS NULL AND (@After IS NULL OR Id > @After) ORDER BY Id",
			new { Take = page.PageSize + PageRequest.LookaheadSize, page.After }, cancellationToken: ct));
		var items = result.AsList();
		var hasMore = items.Count > page.PageSize;
		if (hasMore) items.RemoveAt(items.Count - 1);
		return new(items, hasMore ? items[^1].Id : null);
	}

	public async Task AddAsync(Customer entity, CancellationToken ct)
		=> await context.Customers.AddAsync(entity, ct);

	public void Remove(Customer entity)
		=> context.Customers.Remove(entity);

	public void SetOriginalRowVersion(Customer entity, byte[] rowVersion)
		=> context.Entry(entity).Property(x => x.RowVersion).OriginalValue = rowVersion;

	public async Task SaveChangesAsync(CancellationToken ct)
		=> await context.SaveChangesAsync(ct);
}
```

## IDbConnectionFactory
```csharp
public interface IDbConnectionFactory
{
	IDbConnection CreateConnection();
}

public class SqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
	public IDbConnection CreateConnection()
		=> new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
}
```
Registered as a **Singleton** — it only holds a connection string.

## Dapper multi-mapping (JOIN)
Used when the entity has navigation properties (e.g. Workload → Customer + Employee):
```csharp
var result = await db.QueryAsync<Workload, Customer, Employee, Workload>(
	sql, (w, c, e) => { w.Customer = c; w.Employee = e; return w; },
	splitOn: "Id,Id");
```

## DI registration
```csharp
services.AddScoped<ICustomerRepository, CustomerRepository>();
services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
```

## Important
- `tracked = false` (default) → Dapper, no EF overhead
- `tracked = true` → EF Core, required for Update/Delete so the interceptor can track changes
- Soft delete is filtered manually in Dapper queries (`WHERE Deleted IS NULL`) — EF handles it via `HasQueryFilter`
- All Dapper calls use `CommandDefinition` and propagate the `CancellationToken`
- Lists are bounded and use UUID v7 keyset pagination: default 50, max 100, plus a lookahead row for `NextCursor`
- Also filter soft-deleted joined entities and select explicit columns
- Editable entities include `RowVersion` in Dapper reads; update sets the client's token as the EF original value
- Catch `DbUpdateConcurrencyException` in the handler and return `Result<T>.Conflict`, which the endpoint maps to 409
