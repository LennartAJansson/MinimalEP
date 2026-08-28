namespace MinimalEP.Features.Workload.GetWorkloads;

using MinimalEP.Domain.Core;
using MinimalEP.Features.Core;

public class GetWorkloadsHandler(IWorkloadRepository repository, IUserContext userContext)
  : IRequestHandler<GetWorkloadsRequest, Result<GetWorkloadsResponse>>
{
  public async Task<Result<GetWorkloadsResponse>> HandleAsync(GetWorkloadsRequest request, CancellationToken cancellationToken)
  {
    // A plain User may only ever see their own workloads — never trust a client-supplied
    // EmployeeId for authorization. Admin/SuperAdmin may query freely.
    var isPrivileged = userContext.IsInRole(Roles.SuperAdmin) || userContext.IsInRole(Roles.Admin);
    var effectiveRequest = isPrivileged ? request : request with { EmployeeId = userContext.UserId, CustomerId = null };

    var workloads = effectiveRequest switch
    {
      { CustomerId: not null } => await repository.GetByCustomerAsync(effectiveRequest.CustomerId.Value, cancellationToken),
      { EmployeeId: not null } => await repository.GetByEmployeeAsync(effectiveRequest.EmployeeId.Value, cancellationToken),
      _                        => await repository.GetAllAsync(cancellationToken)
    };

    return new Result<GetWorkloadsResponse>.Ok(workloads.ToResponse());
  }
}
