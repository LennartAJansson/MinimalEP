# Vertical Slice Architecture — Minimal API

## Syfte
Varje use case är en självständig "skiva" genom alla lager — request in, response ut. Inga delade services mellan slices.

## Filstruktur per slice
```
Features/{Aggregate}/{UseCase}/
  {UseCase}Request.cs
  {UseCase}Response.cs
  {UseCase}Validator.cs     ← utelämnas om ingen validering behövs
  {UseCase}Mapping.cs
  {UseCase}Handler.cs
  {UseCase}Endpoint.cs
```

## Request
```csharp
public record AddCustomerRequest(string Name, string Email);
```

## Response
```csharp
public record AddCustomerResponse(Guid Id, string Name, string Email);
```

## Validator (FluentValidation)
```csharp
public class AddCustomerValidator : AbstractValidator<AddCustomerRequest>
{
	public AddCustomerValidator()
	{
		RuleFor(x => x.Name).NotEmpty();
		RuleFor(x => x.Email).NotEmpty().EmailAddress();
	}
}
```
Registreras automatiskt via `AddValidatorsFromAssemblyContaining`. Körs av `ValidationFilter<T>` — returnerar `400 ValidationProblem` vid fel.

## Mapping (C# 14 extension blocks)
```csharp
public static class AddCustomerMapping
{
	extension(AddCustomerRequest request)
	{
		public Customer ToEntity() => new() { Name = request.Name, Email = request.Email };
	}
	extension(Customer customer)
	{
		public AddCustomerResponse ToResponse() => new(customer.Id, customer.Name, customer.Email);
	}
}
```

## Handler
```csharp
public class AddCustomerHandler(ICustomerRepository repository)
	: IRequestHandler<AddCustomerRequest, Result<AddCustomerResponse>>
{
	public async Task<Result<AddCustomerResponse>> HandleAsync(
		AddCustomerRequest request, CancellationToken cancellationToken)
	{
		if (await repository.EmailExistsAsync(request.Email, cancellationToken))
			return new Result<AddCustomerResponse>.Conflict($"Email '{request.Email}' already exists.");

		var customer = request.ToEntity();
		await repository.AddAsync(customer, cancellationToken);
		await repository.SaveChangesAsync(cancellationToken);

		return new Result<AddCustomerResponse>.Ok(customer.ToResponse());
	}
}
```

## Endpoint
```csharp
public class AddCustomerEndpoint : IEndpoint
{
	public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		return builder.MapPost("/customers", async (
			AddCustomerRequest request,
			IRequestHandler<AddCustomerRequest, Result<AddCustomerResponse>> handler,
			CancellationToken cancellationToken) =>
		{
			var result = await handler.HandleAsync(request, cancellationToken);

			IResult httpResult = result switch
			{
				Result<AddCustomerResponse>.Ok ok      => TypedResults.Created($"/customers/{ok.Value.Id}", ok.Value),
				Result<AddCustomerResponse>.Conflict c => TypedResults.Conflict(c.Message),
				_                                      => throw new UnreachableException()
			};

			return httpResult;
		});
	}
}
```

## HTTP Status-konventioner
| Operation  | Lyckat        | Saknas       | Konflikt     |
|------------|---------------|--------------|--------------|
| POST       | 201 Created   | —            | 409 Conflict |
| GET (ett)  | 200 Ok        | 404 NotFound | —            |
| GET (lista)| 200 Ok        | —            | —            |
| PUT/PATCH  | 200 Ok        | 404 NotFound | 409 Conflict |
| DELETE     | 204 NoContent | 404 NotFound | —            |

## Viktigt
- Returnera alltid till en `IResult`-variabel före `return` — löser delegate-tvetydighet med `TypedResults`
- Auth-endpoints: lägg `.AllowAnonymous()` direkt på `MapPost(...)` — överstyr gruppens `RequireAuthorization()`
