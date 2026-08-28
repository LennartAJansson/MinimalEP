namespace MinimalEP.Features.Auth.Register;

public record RegisterRequest(
  string Email,
  string Password,
  string GivenName,
  string Surname,
  int Age,
  string Position,
  string PhoneNumber,
  string Street,
  string PostalCode,
  string City);
