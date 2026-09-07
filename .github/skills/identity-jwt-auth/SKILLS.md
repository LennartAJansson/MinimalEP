# ASP.NET Core Identity + JWT + Refresh Token

## Purpose
Authentication with JWT and refresh tokens. `ApplicationUser.Id == Employee.Id` by design — one person, one Guid.

## ApplicationUser
```csharp
public class ApplicationUser : IdentityUser<Guid>
{
}
```
No refresh-token fields on `ApplicationUser` — see the separate `RefreshToken` entity below.
This supports multiple concurrent sessions/devices per user, revocation of a single
token, and reuse detection (an already rotated/revoked token presented again indicates theft).

## RefreshToken — dedicated entity/table
```csharp
public class RefreshToken : BaseEntity
{
	public required Guid UserId { get; set; }
	public required string TokenHash { get; set; }   // SHA-256, never plaintext
	public required DateTimeOffset ExpiresAt { get; set; }
	public DateTimeOffset? RevokedAt { get; set; }
	public Guid? ReplacedByTokenId { get; set; }
	public required Guid FamilyId { get; set; }
	public byte[] RowVersion { get; set; } = [];

	public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
}
```
`IRefreshTokenRepository` follows the same pattern as the other repositories (`GetActiveByTokenHashAsync`,
`AddAsync`, `SaveChangesAsync`). Always read tracked (EF Core), because rotation requires an update
(`RevokedAt`/`ReplacedByTokenId`) of the existing row.

## DbContext
```csharp
public class ApplicationDbContext
	: IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
```

## Employee.Email — mirror of ApplicationUser.Email
`Employee.Email` mirrors `ApplicationUser.Email` (set at registration, see `RegisterHandler`).
This makes the Employee entity independently readable (e.g. in reports/lists) without joining `AspNetUsers`,
without making `AspNetUsers` the source of domain data. `ApplicationUser.Email` remains the source of truth for login.

## JWT claims
| Claim     | Value |
|-----------|-------|
| `sub`     | `user.Id` — primary identity, used by the interceptor and `IUserContext` |
| `email`   | `user.Email` (from AspNetUsers — the source of truth for login) |
| `jti`     | `Guid.CreateVersion7()` — token id for possible revocation |
| `name`    | `employee.Name` — computed property (`$"{GivenName} {Surname}"`), not persisted |
| `age`     | `employee.Age` |
| `position`| `employee.Position` |
| roles     | `UserManager.GetRolesAsync()` |

`ITokenService.GenerateAccessToken(ApplicationUser user, Employee employee, IList<string> roles)` takes
both `ApplicationUser` and `Employee` because claims come from both entities.

## IUserContext
```csharp
public interface IUserContext
{
	Guid? UserId { get; }
	bool IsInRole(string role);
}
```
Resolves the `sub` claim from `IHttpContextAccessor`. Used by the interceptor and handlers — no separate `EmployeeId` claim is needed. `IsInRole` is used for resource-based authorization (e.g. workload ownership, `/me` scoping) in addition to policy-based routing.

## Important: `MapInboundClaims = false` — consistent claim type everywhere
`JwtBearerOptions.MapInboundClaims` defaults to `false` in ASP.NET Core, so a JWT with a `sub` claim
surfaces as `JwtRegisteredClaimNames.Sub` in `HttpContext.User` (not `ClaimTypes.NameIdentifier`).
`JwtSecurityTokenHandler.ValidateToken()` (used manually, e.g. in the refresh flow), on the other hand, defaults
`MapInboundClaims` to `true` and remaps `sub` to `ClaimTypes.NameIdentifier`.

This creates a silent inconsistency unless you are explicit. Resolve it by:
1. Setting `options.MapInboundClaims = false;` in `AddJwtBearer(...)`.
2. Setting `new JwtSecurityTokenHandler { MapInboundClaims = false }` when validating tokens manually
	 (e.g. in `RefreshTokenHandler` when validating the expired access token).
3. Reading the claim as `principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value`
	 in all consumers (`IUserContext`, the audit interceptor, the refresh handler) as a fallback safeguard.

## RegisterHandler — same Guid for User and Employee
```csharp
var userId = Guid.CreateVersion7();
var user   = new ApplicationUser { Id = userId, UserName = ..., Email = ... };

await userManager.CreateAsync(user, request.Password);

var employee = new Employee { Id = userId, ... };
employee.CreatedBy = userId;   // explicit — no JWT exists at registration

await employeeRepository.AddAsync(employee, ct);
await employeeRepository.SaveChangesAsync(ct);
```

## Token generation (ITokenService)
```csharp
string accessToken  = tokenService.GenerateAccessToken(user, employee, roles);
string refreshToken = tokenService.GenerateRefreshToken();
```
Store the refresh token **hashed** (SHA-256) as a new `RefreshToken` row via `IRefreshTokenRepository`
— not plaintext, and not as a column on `ApplicationUser`.

## Refresh token — rotation
1. Validate the expired access token (`ValidateLifetime = false`) to extract `sub`
2. Look up the `RefreshToken` row via the hash of the submitted refresh token and check `IsActive`
3. If a previous token is reused: revoke the entire token family and log the security event without raw tokens or email addresses
4. Issue a new access token + new refresh token in the same family
5. Set `RevokedAt` + `ReplacedByTokenId` on the old row, add the new row and save atomically
6. `RowVersion` protects against parallel rotation; handle `DbUpdateConcurrencyException` as an invalid/reused token

## Bootstrap, lockout and rate limiting
- Public registration always assigns only `User`.
- SuperAdmin is created only by explicit `BootstrapAdminOptions`, which is disabled by default, startup-validated and allowed only against an empty installation.
- Account, role and Employee are created within an explicit transaction; check every `IdentityResult` and roll back on failure.
- Login uses Identity lockout (`lockoutOnFailure`) and auth endpoints use `RateLimitPolicies.Authentication`.
- Protect the last SuperAdmin and forbid self-demotion.

## Auth endpoints — AllowAnonymous
```csharp
return builder.MapPost("/auth/register", ...)
	.AllowAnonymous();
```
The rest of the route group is protected by `.RequireAuthorization()` — AllowAnonymous overrides it per endpoint.
