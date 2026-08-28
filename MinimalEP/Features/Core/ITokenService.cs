namespace MinimalEP.Features.Core;

using MinimalEP.Domain.Model;

public interface ITokenService
{
  string GenerateAccessToken(ApplicationUser user, Employee employee, IList<string> roles);
  string GenerateRefreshToken();
}
