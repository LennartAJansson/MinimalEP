namespace MinimalEP.Features.Workload.UpdateWorkload;

using Microsoft.EntityFrameworkCore;

using MinimalEP.Domain.Core;
using MinimalEP.Features.Core;

public class UpdateWorkloadHandler(IWorkloadRepository repository, IUserContext userContext)
  : IRequestHandler<UpdateWorkloadRequest, Result<UpdateWorkloadResponse>>
{
  public async Task<Result<UpdateWorkloadResponse>> HandleAsync(UpdateWorkloadRequest request, CancellationToken cancellationToken)
  {
    var workload = await repository.GetByIdAsync(request.Id, cancellationToken, tracked: true);

    if (workload is null)
      return new Result<UpdateWorkloadResponse>.NotFound();

    var isPrivileged = userContext.IsInRole(Roles.SuperAdmin) || userContext.IsInRole(Roles.Admin);
    if (!isPrivileged && workload.EmployeeId != userContext.UserId)
      return new Result<UpdateWorkloadResponse>.NotFound();

    repository.SetOriginalRowVersion(workload, request.RowVersion);
    request.ApplyTo(workload);
    try
    {
      await repository.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateConcurrencyException)
    {
      return new Result<UpdateWorkloadResponse>.Conflict("The workload was changed by another request. Reload it and try again.");
    }

    return new Result<UpdateWorkloadResponse>.Ok(workload.ToResponse());
  }
}
