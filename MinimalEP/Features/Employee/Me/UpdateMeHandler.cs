namespace MinimalEP.Features.Employee.Me;

using MinimalEP.Domain.Core;
using MinimalEP.Features.Core;

public class UpdateMeHandler(IEmployeeRepository repository, IUserContext userContext)
  : IRequestHandler<UpdateMeRequest, Result<GetMeResponse>>
{
  public async Task<Result<GetMeResponse>> HandleAsync(UpdateMeRequest request, CancellationToken cancellationToken)
  {
    if (userContext.UserId is null)
      return new Result<GetMeResponse>.NotFound();

    // tracked: true — this is a write path, EF Core's change tracker must observe the entity
    // (and its owned Address) for SaveChangesAsync to persist the mutation.
    var employee = await repository.GetByIdAsync(userContext.UserId.Value, cancellationToken, tracked: true);

    if (employee is null)
      return new Result<GetMeResponse>.NotFound();

    request.ApplyTo(employee);
    await repository.SaveChangesAsync(cancellationToken);

    return new Result<GetMeResponse>.Ok(employee.ToGetMeResponse());
  }
}
