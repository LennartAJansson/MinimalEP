namespace MinimalEP.Features.Employee.UpdateEmployee;

public record UpdateEmployeeRequest(Guid Id, string Name, int Age, string Position);
