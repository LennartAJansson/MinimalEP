namespace MinimalEP.Features.Auth.Login;

using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public partial class LoginHandler(
  UserManager<ApplicationUser> userManager,
  SignInManager<ApplicationUser> signInManager,
  IEmployeeRepository employeeRepository,
  IRefreshTokenRepository refreshTokenRepository,
  ITokenService tokenService,
  IOptions<JwtOptions> options,
  TimeProvider timeProvider,
  ILogger<LoginHandler> logger)
  : IRequestHandler<LoginRequest, Result<LoginResponse>>
{
  public async Task<Result<LoginResponse>> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
  {
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user is null)
    {
      LoginFailedForUnknownAccount(logger);
      return new Result<LoginResponse>.Unauthorized("Invalid credentials.");
    }

    var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
    if (!signInResult.Succeeded)
    {
      LoginFailedForUser(logger, user.Id, signInResult.IsLockedOut);
      return new Result<LoginResponse>.Unauthorized("Invalid credentials.");
    }

    var employee = await employeeRepository.GetByIdAsync(user.Id, cancellationToken);
    if (employee is null)
      return new Result<LoginResponse>.Unauthorized("Invalid credentials.");

    var roles = await userManager.GetRolesAsync(user);
    var accessToken = tokenService.GenerateAccessToken(user, employee, roles);
    var refreshToken = tokenService.GenerateRefreshToken();

    var expiresAt = timeProvider.GetUtcNow().AddDays(options.Value.RefreshTokenExpiresInDays);

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

  [LoggerMessage(
    EventId = 3001,
    Level = LogLevel.Warning,
    Message = "Login failed for an unknown account.")]
  private static partial void LoginFailedForUnknownAccount(ILogger logger);

  [LoggerMessage(
    EventId = 3002,
    Level = LogLevel.Warning,
    Message = "Login failed for user {UserId}; locked out: {IsLockedOut}.")]
  private static partial void LoginFailedForUser(ILogger logger, Guid userId, bool isLockedOut);

  private static string HashToken(string token)
  {
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
    return Convert.ToBase64String(bytes);
  }
}
