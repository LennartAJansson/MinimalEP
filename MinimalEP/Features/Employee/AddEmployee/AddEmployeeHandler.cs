namespace MinimalEP.Features.Employee.AddEmployee;

using MinimalEP.Features.Core;

public class AddEmployeeHandler(IEmployeeRepository repository)
  : IRequestHandler<AddEmployeeRequest, Result<AddEmployeeResponse>>
{
  public async Task<Result<AddEmployeeResponse>> HandleAsync(AddEmployeeRequest request, CancellationToken cancellationToken)
  {
    var employee = request.ToEntity();

    await repository.AddAsync(employee, cancellationToken);
    await repository.SaveChangesAsync(cancellationToken);

    return new Result<AddEmployeeResponse>.Ok(employee.ToResponse());
  }
}
