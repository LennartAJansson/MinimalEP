namespace MinimalEP.Features.Employee.GetEmployees;

using MinimalEP.Features.Core;

public class GetEmployeesHandler(IEmployeeRepository repository)
  : IRequestHandler<GetEmployeesRequest, Result<GetEmployeesResponse>>
{
  public async Task<Result<GetEmployeesResponse>> HandleAsync(GetEmployeesRequest request, CancellationToken cancellationToken)
  {
    var employees = await repository.GetAllAsync(cancellationToken);
    return new Result<GetEmployeesResponse>.Ok(employees.ToResponse());
  }
}
