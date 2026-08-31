namespace MinimalEP.Features.Core;

using MinimalEP.Domain.Model;

public interface IWorkloadRepository
{
  Task<Workload?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool tracked = false);
  Task<IReadOnlyList<Workload>> GetAllAsync(CancellationToken cancellationToken);
  Task<PagedResult<Workload>> GetPageAsync(PageRequest page, CancellationToken cancellationToken);
  Task<IReadOnlyList<Workload>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken);
  Task<PagedResult<Workload>> GetByCustomerPageAsync(Guid customerId, PageRequest page, CancellationToken cancellationToken);
  Task<IReadOnlyList<Workload>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken);
  Task<PagedResult<Workload>> GetByEmployeePageAsync(Guid employeeId, PageRequest page, CancellationToken cancellationToken);
  Task<bool> HasOpenWorkloadAsync(Guid employeeId, CancellationToken cancellationToken);
  Task AddAsync(Workload workload, CancellationToken cancellationToken);
  void Remove(Workload workload);
  void SetOriginalRowVersion(Workload workload, byte[] rowVersion);
  Task SaveChangesAsync(CancellationToken cancellationToken);
}
