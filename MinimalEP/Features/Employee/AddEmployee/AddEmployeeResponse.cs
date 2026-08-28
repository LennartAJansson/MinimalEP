namespace MinimalEP.Features.Employee.AddEmployee;

public record AddEmployeeResponse(
  Guid Id,
  string Email,
  string Name,
  int Age,
  string Position,
  string PhoneNumber,
  string Street,
  string PostalCode,
  string City);
