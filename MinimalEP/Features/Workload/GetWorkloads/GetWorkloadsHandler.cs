namespace MinimalEP.Features.Workload.GetWorkloads;

using MinimalEP.Features.Core;

public class GetWorkloadsHandler(IWorkloadRepository repository)
  : IRequestHandler<GetWorkloadsRequest, Result<GetWorkloadsResponse>>
{
  public async Task<Result<GetWorkloadsResponse>> HandleAsync(GetWorkloadsRequest request, CancellationToken cancellationToken)
  {
    var workloads = request switch
    {
      { CustomerId: not null } => await repository.GetByCustomerAsync(request.CustomerId.Value, cancellationToken),
      { EmployeeId: not null } => await repository.GetByEmployeeAsync(request.EmployeeId.Value, cancellationToken),
      _                        => await repository.GetAllAsync(cancellationToken)
    };

    return new Result<GetWorkloadsResponse>.Ok(workloads.ToResponse());
  }
}
