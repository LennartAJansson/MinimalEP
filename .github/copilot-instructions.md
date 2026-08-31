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
Task<PagedResult<T>> GetPageAsync(PageRequest page, CancellationToken ct);
Task AddAsync(T entity, CancellationToken ct);
void Remove(T entity);
void SetOriginalRowVersion(T entity, byte[] rowVersion);
Task SaveChangesAsync(CancellationToken ct);
```

- List reads use bounded UUID v7 keyset pagination through `PageRequest` (default 50, maximum 100) and return `PagedResult<T>` with `NextCursor`.
- Every Dapper operation must use `CommandDefinition` and propagate the request `CancellationToken`.
- Dapper SQL must explicitly filter soft-deleted root and joined entities.
- Prefer explicit column lists over `SELECT *`; include `RowVersion` in reads for editable entities.

---

## EF Core Conventions

- `AddDbContextPool` with `(sp, options)` overload
- `AuditAndSoftDeleteInterceptor` registered as **Singleton** (uses `IHttpContextAccessor`)
- Every `IEntityTypeConfiguration<TEntity>` must include:
  ```csharp
  builder.HasQueryFilter(x => x.Deleted == null);
  ```
- `ApplyConfigurationsFromAssembly` in `OnModelCreating` — never configure inline
- Editable `Customer`, `Employee` and `Workload` entities use SQL Server `rowversion` via `builder.Property(x => x.RowVersion).IsRowVersion()`.
- Read responses expose `RowVersion`; update requests send the last-read value. Set it as EF's original value before saving and map `DbUpdateConcurrencyException` to `409 Conflict`.
- Never regenerate or edit an existing migration to change deployed schema; add a new migration and validate it with SQL Server integration tests.

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

- `ApplicationUser : IdentityUser<Guid>` — no refresh-token fields; refresh tokens live in a dedicated `RefreshToken` entity/table (`IRefreshTokenRepository`), hashed (SHA-256), supporting multi-device sessions, revocation, and reuse detection
- `ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
- `Employee.Email` mirrors `ApplicationUser.Email` (set at registration) — `ApplicationUser.Email` remains the source of truth for login
- JWT claims: `sub` (UserId), `email` (from `ApplicationUser`), `jti`, `name`/`age`/`position` (from `Employee`), roles
- `ITokenService.GenerateAccessToken(ApplicationUser user, Employee employee, IList<string> roles)` requires both entities to build claims
- `Register` creates both `ApplicationUser` and `Employee` with the same `Id`
- Auth endpoints (`/auth/register`, `/auth/login`, `/auth/refresh`) use `.AllowAnonymous()`
- All other endpoints require authorization via `RouteGroupBuilder.RequireAuthorization()`
- Administrative customer/employee operations require `AuthorizationPolicies.AdminOrAbove`; role administration uses `SuperAdminOnly` where applicable.
- Public registration always receives the lowest `User` role. SuperAdmin bootstrap is explicit, disabled by default, validated at startup, and only runs against an empty installation.
- Login uses Identity lockout and auth routes use `RateLimitPolicies.Authentication`.
- Refresh tokens are SHA-256 hashed, grouped into token families, rotated atomically, and protected by `rowversion`; reuse revokes the family.

---

## Scalar / OpenAPI — Bearer Auth

`BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer` adds Bearer security scheme.
API versioning already registers OpenAPI. Attach the transformer with `services.ConfigureAll<OpenApiOptions>(...)`; do not call `AddOpenApi` a second time.
Always initialize `operation.Security ??= []` before adding — it is null by default in OpenApi 2.7.5.

OpenAPI and Scalar are development-only. Versioned documents use `/openapi/v1.json` and Scalar uses `/scalar/v1`.

---

## Observability, Health and Problem Details

- Register `AddProblemDetails()` and `UseExceptionHandler()` for centralized unexpected-error handling.
- `/health/live` is anonymous and process-only; `/health/ready` is anonymous and checks SQL Server.
- OpenTelemetry instruments ASP.NET Core, `HttpClient`, SQL Client and HTTP metrics. OTLP export is enabled when `OTEL_EXPORTER_OTLP_ENDPOINT` is configured.
- Preserve W3C Trace Context. A unique `Activity.TraceId` must flow through incoming requests and child spans and be included in structured logs and Problem Details without exposing tokens, passwords or personal data.
- A tracing backend such as Jaeger should consume OTLP; do not add vendor-specific tracing throughout feature code.

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

`RowVersion` belongs only to editable entities (`Customer`, `Employee`, `Workload`), not to `BaseEntity`.

---

## User Identity Convention

`ApplicationUser.Id == Employee.Id` by design.
The interceptor uses `UserId` (from `sub` claim) as both the audit identity and `Workload.EmployeeId`.
`IUserContext.UserId` is the single source of truth — no separate `EmployeeId` claim needed.

---

## Workload — Punch Clock Convention

`Workload` follows a punch-clock model: two distinct slices, not a generic Add/Update.

- `StartWorkload` (`POST /workloads/start`) — request only carries `Start` (no `Stop`), so an already-closed time entry cannot be represented through this endpoint. `EmployeeId` is always taken from `IUserContext.UserId`, never from the client.
- `StopWorkload` (`PATCH /workloads/{id}/stop`) — the only slice allowed to set `Stop`.
- Domain invariant: an employee may not have more than one open workload (`Stop == null`) at a time — `StartWorkloadHandler` uses `IWorkloadRepository.HasOpenWorkloadAsync`; a filtered unique database index is the concurrency-safe final guard.
- Ownership scoping applies to `Get/GetAll/Update/Stop/Delete`: plain `User` role is restricted to their own `EmployeeId`; `Admin`/`SuperAdmin` are not.
- `UpdateWorkload` cannot set `Stop`; only `StopWorkload` may close an entry.
- Workload update and stop requests require the last-read `RowVersion`; stale writes return `409 Conflict`.

---

## Employee — Profile Fields & Self-Service

`Employee` carries `GivenName`, `Surname`, `Age`, `Position`, `PhoneNumber`, and an owned `Address` (`Street`/`PostalCode`/`City`).
`Name` is a computed property (`$"{GivenName} {Surname}"`), never persisted — mappings, responses, and JWT claims use it for convenience but the source of truth is `GivenName`/`Surname`.

- `Address` is an EF Core **owned type** (`builder.OwnsOne`), stored inline on the `Employees` table as `Address_Street`/`Address_PostalCode`/`Address_City` — no separate table/repository, equality by value (DDD value object).
- Dapper cannot map an owned-type navigation directly: `EmployeeRepository` uses multi-mapping (`QueryAsync<Employee, Address, Employee>`, `splitOn: "Street"`) with aliased columns, mirroring the pattern used by `WorkloadRepository` for `Customer`/`Employee`.
- `/me` (`Features/Employee/Me`) is a self-service slice: requests never take an `Id` from the client — the handler resolves the caller's own `Employee` exclusively via `IUserContext.UserId`, preventing IDOR/broken object-level authorization. `UpdateMeRequest` does carry `RowVersion` for concurrency. Any authenticated user may call `/me`.

---

## Shared Contracts and Verification

- Use `ApiRoutes`, `ApiRouteNames`, `ApiVersions`, authorization/rate-limit constants, typed option `SectionName` constants and domain constraint classes instead of duplicated contract values.
- Use named GET routes plus `TypedResults.CreatedAtRoute`; versioned link generation uses `ApiVersions.V1RouteValue`.
- Keep validators synchronized with EF column lengths through shared constraint classes.
- After code changes, build the solution and run relevant tests. Security, database constraints, pagination, cancellation, concurrency and HTTP contracts require regression tests.
- Performance changes require a measured baseline, profiler evidence, and a same-workload before/after comparison; do not optimize unmeasured assumptions.
