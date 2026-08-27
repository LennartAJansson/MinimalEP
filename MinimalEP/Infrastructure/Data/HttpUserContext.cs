namespace MinimalEP.Infrastructure.Data;

using System.Security.Claims;

using MinimalEP.Domain.Core;

public class HttpUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
  public Guid? UserId
  {
    get
    {
      var value = httpContextAccessor.HttpContext?.User?
          .FindFirst(ClaimTypes.NameIdentifier)?.Value;
      return Guid.TryParse(value, out var id) ? id : null;
    }
  }
}
