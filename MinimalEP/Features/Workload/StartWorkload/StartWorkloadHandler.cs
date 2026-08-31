namespace MinimalEP.Features.Workload.StartWorkload;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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
    if (await repository.HasOpenWorkloadAsync(userContext.UserId.Value, cancellationToken))
      return new Result<StartWorkloadResponse>.Conflict("An open workload already exists. Stop it before starting a new one.");

    var workload = request.ToEntity(userContext.UserId.Value);

    await repository.AddAsync(workload, cancellationToken);
    try
    {
      await repository.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException exception) when
      (exception.InnerException is SqlException { Number: 2601 or 2627 })
    {
      return new Result<StartWorkloadResponse>.Conflict("An open workload already exists. Stop it before starting a new one.");
    }

    return new Result<StartWorkloadResponse>.Ok(workload.ToResponse());
  }
}
