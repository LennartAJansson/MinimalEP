namespace MinimalEP.Features.Employee.UpdateEmployee;

using MinimalEP.Domain.Model;

public static class UpdateEmployeeMapping
{
  extension(UpdateEmployeeRequest request)
  {
    public void ApplyTo(Employee employee)
    {
      employee.GivenName = request.GivenName;
      employee.Surname = request.Surname;
      employee.Age = request.Age;
      employee.Position = request.Position;
      employee.PhoneNumber = request.PhoneNumber;
      employee.Address.Street = request.Street;
      employee.Address.PostalCode = request.PostalCode;
      employee.Address.City = request.City;
    }
  }

  extension(Employee employee)
  {
    public UpdateEmployeeResponse ToResponse()
    {
      return new UpdateEmployeeResponse(
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
}
