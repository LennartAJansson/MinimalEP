namespace MinimalEP.Features.Employee.GetEmployees;

using MinimalEP.Domain.Model;

public static class GetEmployeesMapping
{
  extension(Employee employee)
  {
    public GetEmployeesItemResponse ToItemResponse()
    {
      return new GetEmployeesItemResponse(
        employee.Id,
        employee.Email,
        employee.Name,
        employee.Age,
        employee.Position,
        employee.PhoneNumber,
        employee.Address.Street,
        employee.Address.PostalCode,
        employee.Address.City);
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
