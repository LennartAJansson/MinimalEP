namespace MinimalEP.Features.Employee.UpdateEmployee;

using Microsoft.EntityFrameworkCore;

using MinimalEP.Features.Core;

public class UpdateEmployeeHandler(IEmployeeRepository repository)
  : IRequestHandler<UpdateEmployeeRequest, Result<UpdateEmployeeResponse>>
{
  public async Task<Result<UpdateEmployeeResponse>> HandleAsync(UpdateEmployeeRequest request, CancellationToken cancellationToken)
  {
    var employee = await repository.GetByIdAsync(request.Id, cancellationToken, tracked: true);

    if (employee is null)
      return new Result<UpdateEmployeeResponse>.NotFound();

    repository.SetOriginalRowVersion(employee, request.RowVersion);
    request.ApplyTo(employee);
    try
    {
      await repository.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateConcurrencyException)
    {
      return new Result<UpdateEmployeeResponse>.Conflict("The employee was changed by another request. Reload it and try again.");
    }

    return new Result<UpdateEmployeeResponse>.Ok(employee.ToResponse());
  }
}
