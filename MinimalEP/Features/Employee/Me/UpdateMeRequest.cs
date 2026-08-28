namespace MinimalEP.Features.Employee.Me;

public record UpdateMeRequest(
  string GivenName,
  string Surname,
  int Age,
  string Position,
  string PhoneNumber,
  string Street,
  string PostalCode,
  string City);
