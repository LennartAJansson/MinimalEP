namespace MinimalEP.Features.Employee.GetEmployees;

public record GetEmployeesResponse(IReadOnlyList<GetEmployeesItemResponse> Items, Guid? NextCursor);

public record GetEmployeesItemResponse(
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
