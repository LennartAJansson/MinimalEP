namespace MinimalEP.Features.Core;

using MinimalEP.Domain.Model;

public interface IEmployeeRepository
{
  Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool tracked = false);
  Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken);
  Task AddAsync(Employee employee, CancellationToken cancellationToken);
  void Remove(Employee employee);
  Task SaveChangesAsync(CancellationToken cancellationToken);
}
