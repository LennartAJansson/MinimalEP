namespace MinimalEP.Features.Employee.UpdateEmployee;

using MinimalEP.Features.Core;

public class UpdateEmployeeHandler(IEmployeeRepository repository)
  : IRequestHandler<UpdateEmployeeRequest, Result<UpdateEmployeeResponse>>
{
  public async Task<Result<UpdateEmployeeResponse>> HandleAsync(UpdateEmployeeRequest request, CancellationToken cancellationToken)
  {
    var employee = await repository.GetByIdAsync(request.Id, cancellationToken, tracked: true);

    if (employee is null)
      return new Result<UpdateEmployeeResponse>.NotFound();

    request.ApplyTo(employee);
    await repository.SaveChangesAsync(cancellationToken);

    return new Result<UpdateEmployeeResponse>.Ok(employee.ToResponse());
  }
}
