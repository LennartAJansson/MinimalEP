namespace MinimalEP.Infrastructure.Data.Core;

using Microsoft.EntityFrameworkCore;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Data.Context;

public class RefreshTokenRepository(ApplicationDbContext context) : IRefreshTokenRepository
{
  // Tracked query — refresh-token rotation always needs to update (revoke) the returned entity.
  public async Task<RefreshToken?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
  {
    return await context.RefreshTokens
        .FirstOrDefaultAsync(x => x.TokenHash == tokenHash && x.Deleted == null, cancellationToken);
  }

  public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
  {
    await context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken)
  {
    await context.SaveChangesAsync(cancellationToken);
  }
}
