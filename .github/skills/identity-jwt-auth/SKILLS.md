# ASP.NET Core Identity + JWT + Refresh Token

## Syfte
Autentisering med JWT och refresh tokens. `ApplicationUser.Id == Employee.Id` by design — en person, ett Guid.

## ApplicationUser
```csharp
public class ApplicationUser : IdentityUser<Guid>
{
}
```
Inga refresh-token-fält på `ApplicationUser` — se separat `RefreshToken`-entitet nedan.
Detta stödjer flera samtidiga sessioner/enheter per användare, revocation av ett enskilt
token, och reuse-detection (ett redan roterat/återkallat token som presenteras igen indikerar stöld).

## RefreshToken — egen entitet/tabell
```csharp
public class RefreshToken : BaseEntity
{
	public required Guid UserId { get; set; }
	public required string TokenHash { get; set; }   // SHA-256, aldrig plaintext
	public required DateTimeOffset ExpiresAt { get; set; }
	public DateTimeOffset? RevokedAt { get; set; }
	public Guid? ReplacedByTokenId { get; set; }
	public required Guid FamilyId { get; set; }
	public byte[] RowVersion { get; set; } = [];

	public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
}
```
`IRefreshTokenRepository` följer samma mönster som övriga repositories (`GetActiveByTokenHashAsync`,
`AddAsync`, `SaveChangesAsync`). Läs alltid tracked (EF Core) eftersom rotation kräver en uppdatering
(`RevokedAt`/`ReplacedByTokenId`) av den befintliga posten.

## DbContext
```csharp
public class ApplicationDbContext
	: IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
```

## Employee.Email — spegling av ApplicationUser.Email
`Employee.Email` speglar `ApplicationUser.Email` (satt vid registrering, se `RegisterHandler`).
Detta gör Employee-entiteten självständigt läsbar (t.ex. i rapporter/listor) utan join mot `AspNetUsers`,
utan att göra `AspNetUsers` till källan för domändata. `ApplicationUser.Email` förblir källan vid inloggning.

## JWT-claims
| Claim     | Värde |
|-----------|-------|
| `sub`     | `user.Id` — primär identitet, används av interceptorn och `IUserContext` |
| `email`   | `user.Email` (från AspNetUsers — källan vid inloggning) |
| `jti`     | `Guid.CreateVersion7()` — token-id för möjlig revocation |
| `name`    | `employee.Name` — beräknad property (`$"{GivenName} {Surname}"`), inte persisterad |
| `age`     | `employee.Age` |
| `position`| `employee.Position` |
| roller    | `UserManager.GetRolesAsync()` |

`ITokenService.GenerateAccessToken(ApplicationUser user, Employee employee, IList<string> roles)` tar
både `ApplicationUser` och `Employee` eftersom claims hämtas från båda entiteterna.

## IUserContext
```csharp
public interface IUserContext
{
	Guid? UserId { get; }
	bool IsInRole(string role);
}
```
Löser `sub`-claim från `IHttpContextAccessor`. Används av interceptorn och handlers — ingen separat `EmployeeId`-claim behövs. `IsInRole` används för resource-baserad auktorisering (t.ex. workload-ownership, `/me`-scoping) utöver policy-baserad routning.

## Viktigt: `MapInboundClaims = false` — konsekvent claim-typ överallt
`JwtBearerOptions.MapInboundClaims` är `false` som default i ASP.NET Core, så en JWT med `sub`-claim
surfar som `JwtRegisteredClaimNames.Sub` i `HttpContext.User` (inte `ClaimTypes.NameIdentifier`).
`JwtSecurityTokenHandler.ValidateToken()` (använd manuellt, t.ex. vid refresh-flödet) har däremot
`MapInboundClaims = true` som default och mappar om `sub` till `ClaimTypes.NameIdentifier`.

Detta ger en tyst inkonsekvens om man inte är explicit. Lös det genom att:
1. Sätta `options.MapInboundClaims = false;` i `AddJwtBearer(...)`.
2. Sätta `new JwtSecurityTokenHandler { MapInboundClaims = false }` när tokens valideras manuellt
   (t.ex. i `RefreshTokenHandler` vid validering av det utgångna access-tokenet).
3. Läsa claim som `principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value`
   i alla konsumenter (`IUserContext`, audit-interceptorn, refresh-handlern) som fallback-skydd.

## RegisterHandler — samma Guid för User och Employee
```csharp
var userId = Guid.CreateVersion7();
var user   = new ApplicationUser { Id = userId, UserName = ..., Email = ... };

await userManager.CreateAsync(user, request.Password);

var employee = new Employee { Id = userId, ... };
employee.CreatedBy = userId;   // explicit — inget JWT finns vid registrering

await employeeRepository.AddAsync(employee, ct);
await employeeRepository.SaveChangesAsync(ct);
```

## Token-generering (ITokenService)
```csharp
string accessToken  = tokenService.GenerateAccessToken(user, employee, roles);
string refreshToken = tokenService.GenerateRefreshToken();
```
Spara refresh-tokenet **hashat** (SHA-256) som en ny `RefreshToken`-rad via `IRefreshTokenRepository`
— inte plaintext, och inte som kolumn på `ApplicationUser`.

## Refresh token — rotation
1. Validera det utgångna access-tokenet (`ValidateLifetime = false`) för att hämta `sub`
2. Slå upp `RefreshToken`-raden via hash av det inskickade refresh-tokenet och kontrollera `IsActive`
3. Om ett tidigare token återanvänds: återkalla hela tokenfamiljen och logga säkerhetshändelsen utan rå token eller e-postadress
4. Utfärda nytt access token + nytt refresh token i samma familj
5. Sätt `RevokedAt` + `ReplacedByTokenId` på den gamla raden, lägg till den nya raden och spara atomärt
6. `RowVersion` skyddar parallell rotation; hantera `DbUpdateConcurrencyException` som ogiltigt/återanvänt token

## Bootstrap, lockout och rate limiting
- Publik registrering tilldelar alltid endast `User`.
- SuperAdmin skapas bara av explicit `BootstrapAdminOptions`, som är disabled som default, startupvaliderad och tillåten endast i en tom installation.
- Konto, roll och Employee skapas inom en explicit transaktion; kontrollera varje `IdentityResult` och rulla tillbaka vid fel.
- Login använder Identity lockout (`lockoutOnFailure`) och auth-endpoints använder `RateLimitPolicies.Authentication`.
- Skydda sista SuperAdmin och förbjud självdegradering.

## Auth-endpoints — AllowAnonymous
```csharp
return builder.MapPost("/auth/register", ...)
	.AllowAnonymous();
```
Övrig routegrupp skyddas av `.RequireAuthorization()` — AllowAnonymous överstyr per endpoint.
