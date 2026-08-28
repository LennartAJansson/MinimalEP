namespace MinimalEP.Features.Admin.CreateEmployeeAccount;

public record CreateEmployeeAccountRequest(
  string Email,
  string GivenName,
  string Surname,
  int Age,
  string Position,
  string PhoneNumber,
  string Street,
  string PostalCode,
  string City,
  string Role);
