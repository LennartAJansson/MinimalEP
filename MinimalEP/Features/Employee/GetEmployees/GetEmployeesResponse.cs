namespace MinimalEP.Features.Employee.GetEmployees;

public record GetEmployeesResponse(IReadOnlyList<GetEmployeesItemResponse> Items);

public record GetEmployeesItemResponse(Guid Id, string Name, int Age, string Position);
