namespace MinimalEP.Features.Workload.GetWorkload;

using MinimalEP.Domain.Core;
using MinimalEP.Features.Core;

public class GetWorkloadHandler(IWorkloadRepository repository, IUserContext userContext)
  : IRequestHandler<GetWorkloadRequest, Result<GetWorkloadResponse>>
{
  public async Task<Result<GetWorkloadResponse>> HandleAsync(GetWorkloadRequest request, CancellationToken cancellationToken)
  {
    var workload = await repository.GetByIdAsync(request.Id, cancellationToken);

    if (workload is null)
      return new Result<GetWorkloadResponse>.NotFound();

    var isPrivileged = userContext.IsInRole(Roles.SuperAdmin) || userContext.IsInRole(Roles.Admin);
    if (!isPrivileged && workload.EmployeeId != userContext.UserId)
      return new Result<GetWorkloadResponse>.NotFound();

    return new Result<GetWorkloadResponse>.Ok(workload.ToResponse());
  }
}
