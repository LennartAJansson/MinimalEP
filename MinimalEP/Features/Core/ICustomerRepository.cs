namespace MinimalEP.Features.Core;

using MinimalEP.Domain.Model;

public interface ICustomerRepository
{
  Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool tracked = false);
  Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken);
  Task AddAsync(Customer customer, CancellationToken cancellationToken);
  void Remove(Customer customer);
  Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
  Task SaveChangesAsync(CancellationToken cancellationToken);
}
