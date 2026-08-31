namespace MinimalEP.Features.Workload.StopWorkload;

using Microsoft.EntityFrameworkCore;

using MinimalEP.Domain.Core;
using MinimalEP.Features.Core;

public class StopWorkloadHandler(IWorkloadRepository repository, IUserContext userContext)
  : IRequestHandler<StopWorkloadRequest, Result<StopWorkloadResponse>>
{
  public async Task<Result<StopWorkloadResponse>> HandleAsync(StopWorkloadRequest request, CancellationToken cancellationToken)
  {
    var workload = await repository.GetByIdAsync(request.Id, cancellationToken, tracked: true);

    if (workload is null)
      return new Result<StopWorkloadResponse>.NotFound();

    var isPrivileged = userContext.IsInRole(Roles.SuperAdmin) || userContext.IsInRole(Roles.Admin);
    if (!isPrivileged && workload.EmployeeId != userContext.UserId)
      return new Result<StopWorkloadResponse>.NotFound();

    if (workload.Stop.HasValue)
      return new Result<StopWorkloadResponse>.Conflict("Workload is already stopped.");

    if (request.Stop <= workload.Start)
      return new Result<StopWorkloadResponse>.Conflict("Stop time must be after Start.");

    if (request.RowVersion.Length == 0)
      return new Result<StopWorkloadResponse>.Conflict("A row version is required.");

    repository.SetOriginalRowVersion(workload, request.RowVersion);
    workload.Stop = request.Stop;
    try
    {
      await repository.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateConcurrencyException)
    {
      return new Result<StopWorkloadResponse>.Conflict("The workload was changed by another request. Reload it and try again.");
    }

    return new Result<StopWorkloadResponse>.Ok(new StopWorkloadResponse(workload.Id, workload.Start, workload.Stop.Value, workload.RowVersion));
  }
}
