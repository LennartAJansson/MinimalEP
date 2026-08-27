# Auto-registration of Endpoints, Handlers and Validators

## Purpose
Zero manual DI registration for new slices. Assembly scanning picks everything up automatically.

## Registration in Program.cs
```csharp
builder.Services.AddEndpoints(typeof(Program));
```

## What is registered automatically
| Type | Lifetime | How |
|------|----------|-----|
| `IRequestHandler<TRequest, TResponse>` | Transient | Reflection — `ImplementedInterfaces` |
| `IEndpoint` | Transient | Reflection — `IsAssignableTo(typeof(IEndpoint))` |
| `AbstractValidator<T>` | Scoped | `AddValidatorsFromAssemblyContaining` |

## Endpoint mapping with ValidationFilter
```csharp
app.MapEndpoints(versionedGroup);
```
`MapEndpoints` loops all `IEndpoint` implementations, calls `MapEndpoint(builder)`, and automatically attaches `ValidationFilter<TRequest>` to each endpoint.

## How ValidationFilter is matched to the correct endpoint
The filter is resolved by finding the `IRequestHandler<TRequest,TResponse>` that lives in the **same namespace** as the endpoint class. This ensures each endpoint gets its own `ValidationFilter<TRequest>` — not one picked arbitrarily from the assembly.

```csharp
var endpointNamespace = endpoint.GetType().Namespace;

var handlerInterface = endpoint.GetType().Assembly.DefinedTypes
	.Where(t => t is { IsInterface: false, IsAbstract: false } &&
				t.Namespace == endpointNamespace)
	.SelectMany(t => t.ImplementedInterfaces)
	.FirstOrDefault(i => i.IsGenericType &&
						 i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
```

**Never use `FirstOrDefault` without the namespace filter** — it will always match the first handler in the assembly and attach the wrong validator to every endpoint.

## ValidationFilter behaviour
- Resolves `IValidator<TRequest>` from DI
- Returns `400 ValidationProblem` on failure
- If no validator is registered — pipeline continues without validation

## Protect all routes by default
```csharp
RouteGroupBuilder versionedGroup = app
	.MapGroup("api/v{version:apiVersion}")
	.WithApiVersionSet(apiVersionSet)
	.RequireAuthorization();
```
Auth endpoints override with `.AllowAnonymous()` directly on the route method.

## Rules
- Never manually register handlers, endpoints or validators in Program.cs
- Every new slice is picked up automatically as long as the files exist in the assembly
- Slices do not have to be CRUD — model use cases, not entities
