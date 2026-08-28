namespace MinimalEP.Features.Employee.Me;

// No Id in the request — "Me" always resolves the current user's own Employee record
// via IUserContext.UserId, never from a client-supplied id (prevents IDOR/broken object
// level authorization: a user can only ever act on their own profile through this slice).
public record GetMeResponse(
  Guid Id,
  string Email,
  string Name,
  int Age,
  string Position,
  string PhoneNumber,
  string Street,
  string PostalCode,
  string City);
