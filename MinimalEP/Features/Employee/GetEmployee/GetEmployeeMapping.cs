namespace MinimalEP.Features.Employee.GetEmployee;

using MinimalEP.Domain.Model;

public static class GetEmployeeMapping
{
  extension(Employee employee)
  {
    public GetEmployeeResponse ToResponse()
    {
      return new GetEmployeeResponse(employee.Id, employee.Name, employee.Age, employee.Position);
    }
  }
}
