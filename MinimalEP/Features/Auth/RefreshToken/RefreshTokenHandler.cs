namespace MinimalEP.Features.Auth.RefreshToken;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;

public class RefreshTokenHandler(
  UserManager<ApplicationUser> userManager,
  ITokenService tokenService,
  IConfiguration configuration)
  : IRequestHandler<RefreshTokenRequest, Result<RefreshTokenResponse>>
{
  public async Task<Result<RefreshTokenResponse>> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
  {
    // Validera det utgångna access-tokenet för att hämta ut användar-id
    var jwtSettings = configuration.GetSection("Jwt");
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

    var tokenHandler = new JwtSecurityTokenHandler();
    ClaimsPrincipal principal;
    try
    {
      principal = tokenHandler.ValidateToken(request.AccessToken, new TokenValidationParameters
      {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = false   // Tillåt utgångna tokens vid refresh
      }, out _);
    }
    catch
    {
      return new Result<RefreshTokenResponse>.Conflict("Invalid access token.");
    }

    var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (userId is null)
      return new Result<RefreshTokenResponse>.Conflict("Invalid access token.");

    var user = await userManager.FindByIdAsync(userId);
    if (user is null
        || user.RefreshToken != request.RefreshToken
        || user.RefreshTokenExpiry <= DateTimeOffset.UtcNow)
      return new Result<RefreshTokenResponse>.Conflict("Invalid or expired refresh token.");

    var roles = await userManager.GetRolesAsync(user);
    var newAccessToken = tokenService.GenerateAccessToken(user, roles);
    var newRefreshToken = tokenService.GenerateRefreshToken();

    var expiresInDays = int.Parse(configuration["Jwt:RefreshTokenExpiresInDays"] ?? "7");
    user.RefreshToken = newRefreshToken;
    user.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(expiresInDays);
    await userManager.UpdateAsync(user);

    return new Result<RefreshTokenResponse>.Ok(new RefreshTokenResponse(
      newAccessToken,
      newRefreshToken,
      user.RefreshTokenExpiry.Value));
  }
}
