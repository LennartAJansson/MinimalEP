namespace MinimalEP.Features.Core;

using MinimalEP.Domain.Model;

public interface ITokenService
{
  string GenerateAccessToken(ApplicationUser user, IList<string> roles);
  string GenerateRefreshToken();
}
