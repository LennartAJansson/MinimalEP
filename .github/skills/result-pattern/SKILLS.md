# Result Pattern — Typat utfallshantering

## Syfte
Typat diskriminerat fackförbund som ersätter exceptions och nullable returns för förväntade utfall. Handlers returnerar alltid `Result<T>` — endpoints matchar mot HTTP-statuskoder.

## Definition
```csharp
public abstract record Result<T>
{
	public sealed record Ok(T Value) : Result<T>;
	public sealed record NotFound() : Result<T>;
	public sealed record Conflict(string Message) : Result<T>;
}

// Används som TResponse när ingen data returneras (t.ex. Delete)
public record struct Unit
{
	public static readonly Unit Value = new();
}
```

## Handler — returnera Result
```csharp
// Hittades inte
return new Result<CustomerResponse>.NotFound();

// Konflikt (dubblett, affärsregel)
return new Result<CustomerResponse>.Conflict("Email already exists.");

// Optimistic concurrency
catch (DbUpdateConcurrencyException)
{
	return new Result<CustomerResponse>.Conflict(
		"The resource was changed by another request. Reload it and try again.");
}

// Lyckat
return new Result<CustomerResponse>.Ok(customer.ToResponse());
```

## Endpoint — matcha till HTTP
```csharp
IResult httpResult = result switch
{
	Result<CustomerResponse>.Ok ok      => TypedResults.Ok(ok.Value),
	Result<CustomerResponse>.NotFound   => TypedResults.NotFound(),
	Result<CustomerResponse>.Conflict c => TypedResults.Conflict(c.Message),
	_                                   => throw new UnreachableException()
};
return httpResult;
```

## Delete med Unit
```csharp
// Handler
return new Result<Unit>.Ok(Unit.Value);

// Endpoint
Result<Unit>.Ok => TypedResults.NoContent(),
```

## Viktigt
- Tilldela alltid till `IResult`-variabel — löser kompilatorns delegate-tvetydighet när `TypedResults`-subtyper skiljer sig
- `UnreachableException` i `_`-armen garanterar att nya Result-case inte tyst ignoreras
- Lägg till nya case i `Result<T>` (t.ex. `Unauthorized`, `Forbidden`) efter behov
- Förväntade optimistic-concurrency-konflikter mappas till `409 Conflict`; låt inte `DbUpdateConcurrencyException` bli ett generiskt 500-svar
- Central `UseExceptionHandler`/Problem Details hanterar oväntade fel, inte normala domänutfall
