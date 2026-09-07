# Result Pattern — Typed Outcome Handling

## Purpose
A typed discriminated union that replaces exceptions and nullable returns for expected outcomes. Handlers always return `Result<T>` — endpoints match it to HTTP status codes.

## Definition
```csharp
public abstract record Result<T>
{
	public sealed record Ok(T Value) : Result<T>;
	public sealed record NotFound() : Result<T>;
	public sealed record Conflict(string Message) : Result<T>;
}

// Used as TResponse when no data is returned (e.g. Delete)
public record struct Unit
{
	public static readonly Unit Value = new();
}
```

## Handler — return a Result
```csharp
// Not found
return new Result<CustomerResponse>.NotFound();

// Conflict (duplicate, business rule)
return new Result<CustomerResponse>.Conflict("Email already exists.");

// Optimistic concurrency
catch (DbUpdateConcurrencyException)
{
	return new Result<CustomerResponse>.Conflict(
		"The resource was changed by another request. Reload it and try again.");
}

// Success
return new Result<CustomerResponse>.Ok(customer.ToResponse());
```

## Endpoint — match to HTTP
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

## Important
- Always assign to an `IResult` variable — resolves the compiler's delegate ambiguity when `TypedResults` subtypes differ
- `UnreachableException` in the `_` arm guarantees that new Result cases are not silently ignored
- Add new cases to `Result<T>` (e.g. `Unauthorized`, `Forbidden`) as needed
- Expected optimistic-concurrency conflicts map to `409 Conflict`; do not let `DbUpdateConcurrencyException` become a generic 500 response
- Central `UseExceptionHandler`/Problem Details handles unexpected errors, not normal domain outcomes
