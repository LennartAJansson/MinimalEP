namespace MinimalEP.Features.Workload.DeleteWorkload;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class DeleteWorkloadHandler(IWorkloadRepository repository)
  : IRequestHandler<DeleteWorkloadRequest, Result<Unit>>
{
  public async Task<Result<Unit>> HandleAsync(DeleteWorkloadRequest request, CancellationToken cancellationToken)
  {
    var workload = await repository.GetByIdAsync(request.Id, cancellationToken, tracked: true);

    if (workload is null)
      return new Result<Unit>.NotFound();

    repository.Remove(workload);
    await repository.SaveChangesAsync(cancellationToken);

    return new Result<Unit>.Ok(Unit.Value);
  }
}
