namespace MinimalEP.Features.Employee.UpdateEmployee;

public record UpdateEmployeeRequest(
  Guid Id,
  string GivenName,
  string Surname,
  int Age,
  string Position,
  string PhoneNumber,
  string Street,
  string PostalCode,
  string City);
