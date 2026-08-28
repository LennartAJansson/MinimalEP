namespace MinimalEP.Infrastructure.Data;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using MinimalEP.Domain.Core;

public class HttpUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
  public Guid? UserId
  {
    get
    {
      var user = httpContextAccessor.HttpContext?.User;

      // JwtBearer may or may not map "sub" to ClaimTypes.NameIdentifier depending on
      // MapInboundClaims configuration, so check both to be resilient.
      var value = user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

      return Guid.TryParse(value, out var id) ? id : null;
    }
  }

  public bool IsInRole(string role)
  {
    return httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
  }
}
