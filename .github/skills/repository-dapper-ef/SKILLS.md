# Repository Pattern med EF Core + Dapper

## Syfte
Separation of Concerns: Dapper för läsning (snabb, ingen overhead), EF Core för skrivning (interceptor kräver change tracker).

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
Registreras som **Singleton** — håller bara connection string.

## Dapper multi-mapping (JOIN)
Används när entiteten har navigationsegenskaper (t.ex. Workload → Customer + Employee):
```csharp
var result = await db.QueryAsync<Workload, Customer, Employee, Workload>(
	sql, (w, c, e) => { w.Customer = c; w.Employee = e; return w; },
	splitOn: "Id,Id");
```

## DI-registrering
```csharp
services.AddScoped<ICustomerRepository, CustomerRepository>();
services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
```

## Viktigt
- `tracked = false` (default) → Dapper, ingen EF overhead
- `tracked = true` → EF Core, krävs för Update/Delete så interceptorn kan spåra ändringar
- Soft-delete filtreras manuellt i Dapper-queries (`WHERE Deleted IS NULL`) — EF hanterar det via `HasQueryFilter`
- Alla Dapper-anrop använder `CommandDefinition` och förmedlar `CancellationToken`
- Listor är bounded och använder UUID v7 keyset-pagination: default 50, max 100, plus en lookahead-rad för `NextCursor`
- Filtrera även soft-deletade join-entiteter och välj explicita kolumner
- Editable entities inkluderar `RowVersion` i Dapper-läsningar; update sätter klientens token som EF original value
- Fånga `DbUpdateConcurrencyException` i handlern och returnera `Result<T>.Conflict`, vilket endpointen mappar till 409
