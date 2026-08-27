namespace MinimalEP.Features.Employee.AddEmployee;

using MinimalEP.Domain.Model;

public static class AddEmployeeMapping
{
  extension(AddEmployeeRequest request)
  {
    public Employee ToEntity()
    {
      return new Employee
      {
        Name = request.Name,
        Age = request.Age,
        Position = request.Position
      };
    }
  }

  extension(Employee employee)
  {
    public AddEmployeeResponse ToResponse()
    {
      return new AddEmployeeResponse(employee.Id, employee.Name, employee.Age, employee.Position);
    }
  }
}
