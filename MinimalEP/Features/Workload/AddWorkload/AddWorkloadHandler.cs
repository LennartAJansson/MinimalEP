namespace MinimalEP.Features.Workload.AddWorkload;

using MinimalEP.Domain.Core;
using MinimalEP.Features.Core;

public class AddWorkloadHandler(IWorkloadRepository repository, IUserContext userContext)
  : IRequestHandler<AddWorkloadRequest, Result<AddWorkloadResponse>>
{
  public async Task<Result<AddWorkloadResponse>> HandleAsync(AddWorkloadRequest request, CancellationToken cancellationToken)
  {
    if (userContext.UserId is null)
      return new Result<AddWorkloadResponse>.Conflict("The authenticated user has no valid identity.");

    var workload = request.ToEntity(userContext.UserId.Value);

    await repository.AddAsync(workload, cancellationToken);
    await repository.SaveChangesAsync(cancellationToken);

    return new Result<AddWorkloadResponse>.Ok(workload.ToResponse());
  }
}
