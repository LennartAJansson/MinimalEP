namespace MinimalEP.Features.Employee.GetEmployee;

using MinimalEP.Features.Core;

public class GetEmployeeHandler(IEmployeeRepository repository)
  : IRequestHandler<GetEmployeeRequest, Result<GetEmployeeResponse>>
{
  public async Task<Result<GetEmployeeResponse>> HandleAsync(GetEmployeeRequest request, CancellationToken cancellationToken)
  {
    var employee = await repository.GetByIdAsync(request.Id, cancellationToken);

    return employee is null
      ? new Result<GetEmployeeResponse>.NotFound()
      : new Result<GetEmployeeResponse>.Ok(employee.ToResponse());
  }
}
