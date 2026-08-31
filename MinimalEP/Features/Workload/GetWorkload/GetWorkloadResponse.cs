namespace MinimalEP.Features.Workload.GetWorkload;

public record GetWorkloadResponse(Guid Id, Guid CustomerId, string CustomerName, Guid EmployeeId, string EmployeeName, DateTimeOffset Start, DateTimeOffset? Stop, string? Comments, byte[] RowVersion);
