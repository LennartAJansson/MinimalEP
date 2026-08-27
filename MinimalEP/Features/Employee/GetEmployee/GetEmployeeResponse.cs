namespace MinimalEP.Features.Employee.GetEmployee;

public record GetEmployeeResponse(Guid Id, string Name, int Age, string Position);
