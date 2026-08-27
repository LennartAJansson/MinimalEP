namespace MinimalEP.Features.Employee.GetEmployees;

using MinimalEP.Domain.Model;

public static class GetEmployeesMapping
{
  extension(Employee employee)
  {
    public GetEmployeesItemResponse ToItemResponse()
    {
      return new GetEmployeesItemResponse(employee.Id, employee.Name, employee.Age, employee.Position);
    }
  }

  extension(IReadOnlyList<Employee> employees)
  {
    public GetEmployeesResponse ToResponse()
    {
      return new GetEmployeesResponse(employees.Select(e => e.ToItemResponse()).ToList());
    }
  }
}
