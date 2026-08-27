namespace MinimalEP.Features.Auth.Login;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;

public class LoginHandler(
  UserManager<ApplicationUser> userManager,
  ITokenService tokenService,
  IConfiguration configuration)
  : IRequestHandler<LoginRequest, Result<LoginResponse>>
{
  public async Task<Result<LoginResponse>> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
  {
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
      return new Result<LoginResponse>.NotFound();

    var roles = await userManager.GetRolesAsync(user);
    var accessToken = tokenService.GenerateAccessToken(user, roles);
    var refreshToken = tokenService.GenerateRefreshToken();

    var expiresInDays = int.Parse(configuration["Jwt:RefreshTokenExpiresInDays"] ?? "7");
    user.RefreshToken = refreshToken;
    user.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(expiresInDays);
    await userManager.UpdateAsync(user);

    return new Result<LoginResponse>.Ok(new LoginResponse(
      accessToken,
      refreshToken,
      user.RefreshTokenExpiry.Value));
  }
}
