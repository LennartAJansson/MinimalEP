namespace MinimalEP.Features.Core;

using MinimalEP.Domain.Model;

public interface IRefreshTokenRepository
{
  Task<RefreshToken?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
  Task RevokeFamilyAsync(Guid familyId, DateTimeOffset revokedAt, CancellationToken cancellationToken);
  Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
  Task SaveChangesAsync(CancellationToken cancellationToken);
}
