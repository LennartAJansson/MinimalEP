namespace MinimalEP.Features.Employee.Me;

using MinimalEP.Domain.Core;
using MinimalEP.Features.Core;

// A parameterless request marker — the handler ignores any client input and only trusts
// IUserContext, matching the pattern used by StartWorkload for EmployeeId.
public record GetMeRequest;

public class GetMeHandler(IEmployeeRepository repository, IUserContext userContext)
  : IRequestHandler<GetMeRequest, Result<GetMeResponse>>
{
  public async Task<Result<GetMeResponse>> HandleAsync(GetMeRequest request, CancellationToken cancellationToken)
  {
    if (userContext.UserId is null)
      return new Result<GetMeResponse>.NotFound();

    var employee = await repository.GetByIdAsync(userContext.UserId.Value, cancellationToken);

    return employee is null
      ? new Result<GetMeResponse>.NotFound()
      : new Result<GetMeResponse>.Ok(employee.ToGetMeResponse());
  }
}
