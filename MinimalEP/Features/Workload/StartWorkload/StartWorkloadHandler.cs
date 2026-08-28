namespace MinimalEP.Features.Workload.StartWorkload;

using MinimalEP.Domain.Core;
using MinimalEP.Features.Core;

public class StartWorkloadHandler(IWorkloadRepository repository, IUserContext userContext)
  : IRequestHandler<StartWorkloadRequest, Result<StartWorkloadResponse>>
{
  public async Task<Result<StartWorkloadResponse>> HandleAsync(StartWorkloadRequest request, CancellationToken cancellationToken)
  {
    if (userContext.UserId is null)
      return new Result<StartWorkloadResponse>.Conflict("The authenticated user has no valid identity.");

    // Punch-clock rule: an employee cannot start a new workload while a previous one is still
    // open (Stop == null). Enforcing this invariant here — not just in the UI — is what keeps
    // the domain consistent regardless of which client calls the API (Single Responsibility:
    // the handler owns the business rule, not the controller/endpoint).
    var existing = await repository.GetByEmployeeAsync(userContext.UserId.Value, cancellationToken);
    if (existing.Any(w => w.Stop is null))
      return new Result<StartWorkloadResponse>.Conflict("An open workload already exists. Stop it before starting a new one.");

    var workload = request.ToEntity(userContext.UserId.Value);

    await repository.AddAsync(workload, cancellationToken);
    await repository.SaveChangesAsync(cancellationToken);

    return new Result<StartWorkloadResponse>.Ok(workload.ToResponse());
  }
}
