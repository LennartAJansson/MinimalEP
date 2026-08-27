namespace MinimalEP.Features.Workload.GetWorkload;

using MinimalEP.Features.Core;

public class GetWorkloadHandler(IWorkloadRepository repository)
  : IRequestHandler<GetWorkloadRequest, Result<GetWorkloadResponse>>
{
  public async Task<Result<GetWorkloadResponse>> HandleAsync(GetWorkloadRequest request, CancellationToken cancellationToken)
  {
    var workload = await repository.GetByIdAsync(request.Id, cancellationToken);

    return workload is null
      ? new Result<GetWorkloadResponse>.NotFound()
      : new Result<GetWorkloadResponse>.Ok(workload.ToResponse());
  }
}
