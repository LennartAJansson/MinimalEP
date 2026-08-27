namespace MinimalEP.Features.Employee.UpdateEmployee;

using MinimalEP.Domain.Model;

public static class UpdateEmployeeMapping
{
  extension(UpdateEmployeeRequest request)
  {
    public void ApplyTo(Employee employee)
    {
      employee.Name = request.Name;
      employee.Age = request.Age;
      employee.Position = request.Position;
    }
  }

  extension(Employee employee)
  {
    public UpdateEmployeeResponse ToResponse()
    {
      return new UpdateEmployeeResponse(employee.Id, employee.Name, employee.Age, employee.Position);
    }
  }
}
