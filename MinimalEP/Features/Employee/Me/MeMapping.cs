namespace MinimalEP.Features.Employee.Me;

using MinimalEP.Domain.Model;

public static class MeMapping
{
  extension(Employee employee)
  {
    public GetMeResponse ToGetMeResponse()
    {
      return new GetMeResponse(
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

  extension(UpdateMeRequest request)
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
}
