# ASP.NET Core Identity + JWT + Refresh Token

## Syfte
Autentisering med JWT och refresh tokens. `ApplicationUser.Id == Employee.Id` by design — en person, ett Guid.

## ApplicationUser
```csharp
public class ApplicationUser : IdentityUser<Guid>
{
	public string? RefreshToken { get; set; }
	public DateTimeOffset? RefreshTokenExpiry { get; set; }
}
```

## DbContext
```csharp
public class ApplicationDbContext
	: IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
```

## JWT-claims
| Claim  | Värde |
|--------|-------|
| `sub`  | `user.Id` — primär identitet, används av interceptorn och `IUserContext` |
| `email`| `user.Email` |
| `jti`  | `Guid.NewGuid()` — token-id för möjlig revocation |
| roller | `UserManager.GetRolesAsync()` |

## IUserContext
```csharp
public interface IUserContext
{
	Guid? UserId { get; }
}
```
Löser `sub`-claim från `IHttpContextAccessor`. Används av interceptorn och handlers — ingen separat `EmployeeId`-claim behövs.

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
string accessToken  = tokenService.GenerateToken(user, roles);
string refreshToken = tokenService.GenerateRefreshToken();
```
Spara `refreshToken` + `RefreshTokenExpiry` på `ApplicationUser` och kalla `userManager.UpdateAsync`.

## Refresh token — rotation
1. Validera att `refreshToken` matchar och inte är expired
2. Utfärda nytt access token + nytt refresh token
3. Uppdatera `ApplicationUser` med det nya refresh token

## Auth-endpoints — AllowAnonymous
```csharp
return builder.MapPost("/auth/register", ...)
	.AllowAnonymous();
```
Övrig routegrupp skyddas av `.RequireAuthorization()` — AllowAnonymous överstyr per endpoint.
