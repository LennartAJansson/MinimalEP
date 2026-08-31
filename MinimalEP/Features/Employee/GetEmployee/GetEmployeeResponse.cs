namespace MinimalEP.Features.Employee.GetEmployee;

public record GetEmployeeResponse(
  Guid Id,
  string Email,
  string Name,
  int Age,
  string Position,
  string PhoneNumber,
  string Street,
  string PostalCode,
  string City,
  byte[] RowVersion);
