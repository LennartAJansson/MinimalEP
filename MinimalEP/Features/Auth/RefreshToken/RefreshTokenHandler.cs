namespace MinimalEP.Features.Auth.RefreshToken;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class RefreshTokenHandler(
  UserManager<ApplicationUser> userManager,
  IEmployeeRepository employeeRepository,
  IRefreshTokenRepository refreshTokenRepository,
  ITokenService tokenService,
  IOptions<JwtOptions> options,
  ILogger<RefreshTokenHandler> logger)
  : IRequestHandler<RefreshTokenRequest, Result<RefreshTokenResponse>>
{
  public async Task<Result<RefreshTokenResponse>> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
  {
    // Validate the expired access token to extract the user id
    var jwtSettings = options.Value;
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));

    var tokenHandler = new JwtSecurityTokenHandler
    {
      // Keep claim types as issued ("sub") instead of the legacy WS-Federation mapping
      // (e.g. "sub" -> ClaimTypes.NameIdentifier), matching JwtBearer's MapInboundClaims = false default.
      MapInboundClaims = false
    };

    ClaimsPrincipal principal;
    try
    {
      principal = tokenHandler.ValidateToken(request.AccessToken, new TokenValidationParameters
      {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = false   // Allow expired tokens when refreshing
      }, out _);
    }
    catch
    {
      return new Result<RefreshTokenResponse>.Conflict("Invalid access token.");
    }

    var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                 ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId is null)
      return new Result<RefreshTokenResponse>.Conflict("Invalid access token.");

    var user = await userManager.FindByIdAsync(userId);
    if (user is null)
      return new Result<RefreshTokenResponse>.Conflict("Invalid or expired refresh token.");

    var employee = await employeeRepository.GetByIdAsync(user.Id, cancellationToken);
    if (employee is null)
      return new Result<RefreshTokenResponse>.Conflict("Invalid or expired refresh token.");

    var tokenHash = HashToken(request.RefreshToken);
    var storedToken = await refreshTokenRepository.GetActiveByTokenHashAsync(tokenHash, cancellationToken);

    // Reuse detection: a token that has already been revoked/rotated but is presented again
    // indicates possible theft. Treat it as invalid rather than silently accepting it.
    if (storedToken is null)
      return new Result<RefreshTokenResponse>.Conflict("Invalid or expired refresh token.");

    if (storedToken.UserId != user.Id || !storedToken.IsActive)
    {
      await refreshTokenRepository.RevokeFamilyAsync(storedToken.FamilyId, DateTimeOffset.UtcNow, cancellationToken);
      logger.LogWarning("Refresh-token reuse detected for family {TokenFamilyId}; the family was revoked.", storedToken.FamilyId);
      return new Result<RefreshTokenResponse>.Conflict("Invalid or expired refresh token.");
    }

    var roles = await userManager.GetRolesAsync(user);
    var newAccessToken = tokenService.GenerateAccessToken(user, employee, roles);
    var newRefreshToken = tokenService.GenerateRefreshToken();

    var expiresAt = DateTimeOffset.UtcNow.AddDays(jwtSettings.RefreshTokenExpiresInDays);

    var newToken = new RefreshToken
    {
      UserId = user.Id,
      FamilyId = storedToken.FamilyId,
      TokenHash = HashToken(newRefreshToken),
      ExpiresAt = expiresAt,
      CreatedBy = user.Id
    };

    // Revoke the old token and link it to its replacement
    storedToken.RevokedAt = DateTimeOffset.UtcNow;
    storedToken.ReplacedByTokenId = newToken.Id;

    await refreshTokenRepository.AddAsync(newToken, cancellationToken);
    try
    {
      await refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateConcurrencyException)
    {
      await refreshTokenRepository.RevokeFamilyAsync(storedToken.FamilyId, DateTimeOffset.UtcNow, cancellationToken);
      logger.LogWarning("Concurrent refresh-token rotation detected for family {TokenFamilyId}; the family was revoked.", storedToken.FamilyId);
      return new Result<RefreshTokenResponse>.Conflict("Invalid or expired refresh token.");
    }

    return new Result<RefreshTokenResponse>.Ok(new RefreshTokenResponse(
      newAccessToken,
      newRefreshToken,
      expiresAt));
  }

  private static string HashToken(string token)
  {
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
    return Convert.ToBase64String(bytes);
  }
}
