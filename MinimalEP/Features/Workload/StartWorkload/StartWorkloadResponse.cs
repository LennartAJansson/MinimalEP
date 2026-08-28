namespace MinimalEP.Features.Workload.StartWorkload;

public record StartWorkloadResponse(Guid Id, Guid CustomerId, Guid EmployeeId, DateTimeOffset Start, string? Comments);
