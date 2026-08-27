namespace MinimalEP.Features.Workload.AddWorkload;

public record AddWorkloadResponse(Guid Id, Guid CustomerId, Guid EmployeeId, DateTimeOffset Start, DateTimeOffset? Stop, string? Comments);
