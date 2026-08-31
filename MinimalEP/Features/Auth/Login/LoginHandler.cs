namespace MinimalEP.Features.Auth.Login;

using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class LoginHandler(
  UserManager<ApplicationUser> userManager,
  SignInManager<ApplicationUser> signInManager,
  IEmployeeRepository employeeRepository,
  IRefreshTokenRepository refreshTokenRepository,
  ITokenService tokenService,
  IOptions<JwtOptions> options,
  ILogger<LoginHandler> logger)
  : IRequestHandler<LoginRequest, Result<LoginResponse>>
{
  public async Task<Result<LoginResponse>> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
  {
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user is null)
    {
      logger.LogWarning("Login failed for an unknown account.");
      return new Result<LoginResponse>.NotFound();
    }

    var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
    if (!signInResult.Succeeded)
    {
      logger.LogWarning("Login failed for user {UserId}; locked out: {IsLockedOut}.", user.Id, signInResult.IsLockedOut);
      return new Result<LoginResponse>.NotFound();
    }

    var employee = await employeeRepository.GetByIdAsync(user.Id, cancellationToken);
    if (employee is null)
      return new Result<LoginResponse>.NotFound();

    var roles = await userManager.GetRolesAsync(user);
    var accessToken = tokenService.GenerateAccessToken(user, employee, roles);
    var refreshToken = tokenService.GenerateRefreshToken();

    var expiresAt = DateTimeOffset.UtcNow.AddDays(options.Value.RefreshTokenExpiresInDays);

    await refreshTokenRepository.AddAsync(new RefreshToken
    {
      UserId = user.Id,
      FamilyId = Guid.CreateVersion7(),
      TokenHash = HashToken(refreshToken),
      ExpiresAt = expiresAt,
      CreatedBy = user.Id
    }, cancellationToken);
    await refreshTokenRepository.SaveChangesAsync(cancellationToken);

    return new Result<LoginResponse>.Ok(new LoginResponse(accessToken, refreshToken, expiresAt));
  }

  private static string HashToken(string token)
  {
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
    return Convert.ToBase64String(bytes);
  }
}
