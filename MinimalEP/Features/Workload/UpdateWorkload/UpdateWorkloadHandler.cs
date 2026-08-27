namespace MinimalEP.Features.Workload.UpdateWorkload;

using MinimalEP.Features.Core;

public class UpdateWorkloadHandler(IWorkloadRepository repository)
  : IRequestHandler<UpdateWorkloadRequest, Result<UpdateWorkloadResponse>>
{
  public async Task<Result<UpdateWorkloadResponse>> HandleAsync(UpdateWorkloadRequest request, CancellationToken cancellationToken)
  {
    var workload = await repository.GetByIdAsync(request.Id, cancellationToken, tracked: true);

    if (workload is null)
      return new Result<UpdateWorkloadResponse>.NotFound();

    request.ApplyTo(workload);
    await repository.SaveChangesAsync(cancellationToken);

    return new Result<UpdateWorkloadResponse>.Ok(workload.ToResponse());
  }
}
