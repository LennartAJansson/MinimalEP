namespace MinimalEP.Features.Core;

using MinimalEP.Domain.Model;

public interface IWorkloadRepository
{
  Task<Workload?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool tracked = false);
  Task<IReadOnlyList<Workload>> GetAllAsync(CancellationToken cancellationToken);
  Task<IReadOnlyList<Workload>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken);
  Task<IReadOnlyList<Workload>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken);
  Task AddAsync(Workload workload, CancellationToken cancellationToken);
  void Remove(Workload workload);
  Task SaveChangesAsync(CancellationToken cancellationToken);
}
