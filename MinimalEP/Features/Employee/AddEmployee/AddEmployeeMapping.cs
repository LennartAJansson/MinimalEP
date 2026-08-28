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
        Email = request.Email,
        GivenName = request.GivenName,
        Surname = request.Surname,
        Age = request.Age,
        Position = request.Position,
        PhoneNumber = request.PhoneNumber,
        Address = new Address
        {
          Street = request.Street,
          PostalCode = request.PostalCode,
          City = request.City
        }
      };
    }
  }

  extension(Employee employee)
  {
    public AddEmployeeResponse ToResponse()
    {
      return new AddEmployeeResponse(
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
