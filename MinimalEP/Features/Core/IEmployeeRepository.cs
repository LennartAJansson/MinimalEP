namespace MinimalEP.Features.Core;

using MinimalEP.Domain.Model;

public interface IEmployeeRepository
{
  Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool tracked = false);
  Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken);
  Task<PagedResult<Employee>> GetPageAsync(PageRequest page, CancellationToken cancellationToken);
  Task AddAsync(Employee employee, CancellationToken cancellationToken);
  void Remove(Employee employee);
  void SetOriginalRowVersion(Employee employee, byte[] rowVersion);
  Task SaveChangesAsync(CancellationToken cancellationToken);
}
