namespace MinimalEP.Features.Employee.GetEmployees;

public record GetEmployeesRequest(int? PageSize, Guid? After);
