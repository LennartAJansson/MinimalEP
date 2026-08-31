namespace MinimalEP.Features.Workload.GetWorkloads;

public record GetWorkloadsRequest(Guid? CustomerId, Guid? EmployeeId, int? PageSize, Guid? After);
