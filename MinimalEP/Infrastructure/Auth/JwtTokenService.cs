namespace MinimalEP.Infrastructure.Auth;

using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;

public class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
{
  private readonly JwtOptions settings = options.Value;

  public string GenerateAccessToken(ApplicationUser user, Employee employee, IList<string> roles)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    List<Claim> claims =
    [
      new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new(JwtRegisteredClaimNames.Email, user.Email!),
      new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
      new(EmployeeClaimNames.Name, employee.Name),
      new(EmployeeClaimNames.Age, employee.Age.ToString(CultureInfo.InvariantCulture)),
      new(EmployeeClaimNames.Position, employee.Position),
      .. roles.Select(r => new Claim(ClaimTypes.Role, r))
    ];

    var token = new JwtSecurityToken(
      issuer: settings.Issuer,
      audience: settings.Audience,
      claims: claims,
      expires: timeProvider.GetUtcNow().AddMinutes(settings.ExpiresInMinutes).UtcDateTime,
      signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public string GenerateRefreshToken()
  {
    var bytes = RandomNumberGenerator.GetBytes(AuthDefaults.RefreshTokenSizeBytes);
    return Convert.ToBase64String(bytes);
  }
}
