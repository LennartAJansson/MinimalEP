namespace MinimalEP.Features.Core;

using MinimalEP.Domain.Model;

public interface ICustomerRepository
{
  Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool tracked = false);
  Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken);
  Task<PagedResult<Customer>> GetPageAsync(PageRequest page, CancellationToken cancellationToken);
  Task AddAsync(Customer customer, CancellationToken cancellationToken);
  void Remove(Customer customer);
  void SetOriginalRowVersion(Customer customer, byte[] rowVersion);
  Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
  Task SaveChangesAsync(CancellationToken cancellationToken);
}
