namespace MinimalEP.Features.Employee.UpdateEmployee;

public record UpdateEmployeeResponse(
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
