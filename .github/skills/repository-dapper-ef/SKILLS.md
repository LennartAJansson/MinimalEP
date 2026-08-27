# Repository Pattern med EF Core + Dapper

## Syfte
Separation of Concerns: Dapper för läsning (snabb, ingen overhead), EF Core för skrivning (interceptor kräver change tracker).

## Interface (Features/Core/)
```csharp
public interface ICustomerRepository
{
	Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct, bool tracked = false);
	Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct);
	Task AddAsync(Customer entity, CancellationToken ct);
	void Remove(Customer entity);
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
		return await db.QuerySingleOrDefaultAsync<Customer>(
			"SELECT * FROM Customers WHERE Id = @Id AND Deleted IS NULL",
			new { Id = id });
	}

	public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct)
	{
		using IDbConnection db = connectionFactory.CreateConnection();
		var result = await db.QueryAsync<Customer>(
			"SELECT * FROM Customers WHERE Deleted IS NULL");
		return result.AsList();
	}

	public async Task AddAsync(Customer entity, CancellationToken ct)
		=> await context.Customers.AddAsync(entity, ct);

	public void Remove(Customer entity)
		=> context.Customers.Remove(entity);

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
