namespace MinimalEP.Features.Auth.Login;

using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;

public class LoginHandler(
  UserManager<ApplicationUser> userManager,
  IEmployeeRepository employeeRepository,
  IRefreshTokenRepository refreshTokenRepository,
  ITokenService tokenService,
  IConfiguration configuration)
  : IRequestHandler<LoginRequest, Result<LoginResponse>>
{
  public async Task<Result<LoginResponse>> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
  {
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
      return new Result<LoginResponse>.NotFound();

    var employee = await employeeRepository.GetByIdAsync(user.Id, cancellationToken);
    if (employee is null)
      return new Result<LoginResponse>.NotFound();

    var roles = await userManager.GetRolesAsync(user);
    var accessToken = tokenService.GenerateAccessToken(user, employee, roles);
    var refreshToken = tokenService.GenerateRefreshToken();

    var expiresInDays = int.Parse(configuration["Jwt:RefreshTokenExpiresInDays"] ?? "7");
    var expiresAt = DateTimeOffset.UtcNow.AddDays(expiresInDays);

    await refreshTokenRepository.AddAsync(new RefreshToken
    {
      UserId = user.Id,
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
