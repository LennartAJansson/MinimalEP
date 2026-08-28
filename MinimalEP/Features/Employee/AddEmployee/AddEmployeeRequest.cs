namespace MinimalEP.Features.Employee.AddEmployee;

public record AddEmployeeRequest(
  string Email,
  string GivenName,
  string Surname,
  int Age,
  string Position,
  string PhoneNumber,
  string Street,
  string PostalCode,
  string City);
