namespace MinimalEP.Features.Employee.DeleteEmployee;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class DeleteEmployeeHandler(IEmployeeRepository repository)
  : IRequestHandler<DeleteEmployeeRequest, Result<Unit>>
{
  public async Task<Result<Unit>> HandleAsync(DeleteEmployeeRequest request, CancellationToken cancellationToken)
  {
    var employee = await repository.GetByIdAsync(request.Id, cancellationToken, tracked: true);

    if (employee is null)
      return new Result<Unit>.NotFound();

    repository.Remove(employee);
    await repository.SaveChangesAsync(cancellationToken);

    return new Result<Unit>.Ok(Unit.Value);
  }
}
