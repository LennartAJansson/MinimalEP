namespace MinimalEP.Features.Workload.UpdateWorkload;

public record UpdateWorkloadResponse(Guid Id, Guid CustomerId, Guid EmployeeId, DateTimeOffset Start, DateTimeOffset? Stop, string? Comments);
