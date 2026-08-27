# MinimalEP — Copilot Instructions

This repository is a **Minimal API template** using **Vertical Slice Architecture** in .NET 10.
When generating or modifying code, always follow the patterns described here.

---

## Architecture Overview

```
Domain/          → Entities, interfaces (no outward dependencies)
Features/        → One folder per aggregate, one subfolder per use case (vertical slice)
  Core/          → Shared abstractions: IEndpoint, IRequestHandler<T,R>, Result<T>, ValidationFilter
Infrastructure/  → EF Core, Dapper, repositories, interceptors, auth, configurations
```

---

## Vertical Slice — File Structure

Every use case lives in its own folder and contains these files:

```
Features/{Aggregate}/{UseCase}/
  {UseCase}Request.cs       → record with input data
  {UseCase}Response.cs      → record with output data
  {UseCase}Validator.cs     → FluentValidation (only if input needs validation)
  {UseCase}Mapping.cs       → extension methods via C# 14 extension blocks
  {UseCase}Handler.cs       → implements IRequestHandler<TRequest, Result<TResponse>>
  {UseCase}Endpoint.cs      → implements IEndpoint, maps HTTP route
```

---

## Core Abstractions

### IEndpoint
```csharp
public interface IEndpoint
{
	IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder);
}
```

### IRequestHandler
```csharp
public interface IRequestHandler<TRequest, TResponse>
{
	Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
```

### Result\<T\>
```csharp
public abstract record Result<T>
{
	public sealed record Ok(T Value) : Result<T>;
	public sealed record NotFound() : Result<T>;
	public sealed record Conflict(string Message) : Result<T>;
}
public record struct Unit { public static readonly Unit Value = new(); }
```

---

## HTTP Status Conventions

| Operation   | Success              | Not found      | Conflict / Invalid |
|-------------|----------------------|----------------|--------------------|
| POST (add)  | 201 Created          | —              | 409 Conflict       |
| GET single  | 200 Ok               | 404 NotFound   | —                  |
| GET list    | 200 Ok               | —              | —                  |
| PUT/PATCH   | 200 Ok               | 404 NotFound   | 409 Conflict       |
| DELETE      | 204 NoContent        | 404 NotFound   | —                  |

Always use `TypedResults` (not `Results`) and assign to `IResult` before returning to resolve delegate ambiguity:
```csharp
IResult httpResult = result switch
{
	Result<T>.Ok ok    => TypedResults.Ok(ok.Value),
	Result<T>.NotFound => TypedResults.NotFound(),
	_                  => throw new UnreachableException()
};
return httpResult;
```

---

## Auto-Registration

`EndpointExtensions.AddEndpoints(typeof(Program))` scans the assembly and automatically registers:
- All `IRequestHandler<,>` implementations (Transient)
- All `IEndpoint` implementations (Transient)
- All FluentValidation `AbstractValidator<>` (via `AddValidatorsFromAssemblyContaining`)

**Never manually register handlers, endpoints or validators in Program.cs.**

---

## Mapping — C# 14 Extension Blocks

```csharp
public static class AddCustomerMapping
{
	extension(AddCustomerRequest request)
	{
		public Customer ToEntity() => new Customer { ... };
	}
	extension(Customer customer)
	{
		public AddCustomerResponse ToResponse() => new(...);
	}
}
```

---

## ValidationFilter

`ValidationFilter<TRequest>` runs automatically via `EndpointExtensions.MapEndpoints()`.
It resolves `IValidator<TRequest>` from DI and returns `400 ValidationProblem` on failure.
No manual filter registration needed.

---

## Repository Pattern

Each aggregate has:
- `IXxxRepository` in `Features/Core/`
- `XxxRepository` in `Infrastructure/Data/Core/`

**Read operations** → Dapper (fast, no change tracking)
**Write operations** → EF Core (interceptor requires change tracker)
**tracked=true** → EF Core query (required before Update/Delete)

```csharp
Task<T?> GetByIdAsync(Guid id, CancellationToken ct, bool tracked = false);
Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct);
Task AddAsync(T entity, CancellationToken ct);
void Remove(T entity);
Task SaveChangesAsync(CancellationToken ct);
```

---

## EF Core Conventions

- `AddDbContextPool` with `(sp, options)` overload
- `AuditAndSoftDeleteInterceptor` registered as **Singleton** (uses `IHttpContextAccessor`)
- Every `IEntityTypeConfiguration<TEntity>` must include:
  ```csharp
  builder.HasQueryFilter(x => x.Deleted == null);
  ```
- `ApplyConfigurationsFromAssembly` in `OnModelCreating` — never configure inline

---

## Audit & Soft Delete — Interceptor

`AuditAndSoftDeleteInterceptor` automatically sets on all `BaseEntity` entries:
- `Added` → `Created = now`, `CreatedBy ??= currentUserId`
- `Modified` → `Updated = now`, `UpdatedBy = currentUserId`
- `Deleted` → converts to `Modified`, sets `Deleted = now`, `DeletedBy = currentUserId`
- `Workload` Added → also sets `EmployeeId = currentUserId` (User.Id == Employee.Id by design)

To set `CreatedBy` when no JWT exists (e.g. registration), set it explicitly before `SaveChanges` — the interceptor uses `??=` and will not overwrite it.

---

## Auth — Identity + JWT

- `ApplicationUser : IdentityUser<Guid>` with `RefreshToken` + `RefreshTokenExpiry`
- `ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
- JWT claims: `sub` (UserId), `email`, `jti`, roles
- `Register` creates both `ApplicationUser` and `Employee` with the same `Id`
- Auth endpoints (`/auth/register`, `/auth/login`, `/auth/refresh`) use `.AllowAnonymous()`
- All other endpoints require authorization via `RouteGroupBuilder.RequireAuthorization()`

---

## Scalar / OpenAPI — Bearer Auth

`BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer` adds Bearer security scheme.
Register via `services.AddOpenApi(o => o.AddDocumentTransformer<BearerSecuritySchemeTransformer>())`.
Always initialize `operation.Security ??= []` before adding — it is null by default in OpenApi 2.7.5.

---

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

---

## User Identity Convention

`ApplicationUser.Id == Employee.Id` by design.
The interceptor uses `UserId` (from `sub` claim) as both the audit identity and `Workload.EmployeeId`.
`IUserContext.UserId` is the single source of truth — no separate `EmployeeId` claim needed.
