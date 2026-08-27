# Auto-registrering av Endpoints, Handlers och Validators

## Syfte
Noll manuell DI-registrering för nya slices. Assembly-scanning plockar upp allt automatiskt.

## Registrering i Program.cs
```csharp
builder.Services.AddEndpoints(typeof(Program));
```

## Vad som registreras automatiskt
| Typ | Livslängd | Hur |
|-----|-----------|-----|
| `IRequestHandler<TRequest, TResponse>` | Transient | Reflection — `ImplementedInterfaces` |
| `IEndpoint` | Transient | Reflection — `IsAssignableTo(typeof(IEndpoint))` |
| `AbstractValidator<T>` | Scoped | `AddValidatorsFromAssemblyContaining` |

## Endpoint-mappning med ValidationFilter
```csharp
app.MapEndpoints(versionedGroup);
```
`MapEndpoints` loopar alla `IEndpoint`, kallar `MapEndpoint(builder)` och lägger automatiskt på `ValidationFilter<TRequest>` per endpoint.

## ValidationFilter
- Löser `IValidator<TRequest>` från DI
- Returnerar `400 ValidationProblem` om validering misslyckas
- Om ingen validator finns — fortsätter pipeline utan validering

## Skydda alla routes som standard
```csharp
RouteGroupBuilder versionedGroup = app
	.MapGroup("api/v{version:apiVersion}")
	.WithApiVersionSet(apiVersionSet)
	.RequireAuthorization();
```
Auth-endpoints överstyr med `.AllowAnonymous()` direkt på route-metoden.

## Viktigt
- Lägg aldrig till handlers, endpoints eller validators manuellt i Program.cs
- Varje ny slice plockas upp automatiskt bara filerna finns i assembly:n
