namespace MinimalEP.Features.Workload.GetWorkloads;

public record GetWorkloadsResponse(IReadOnlyList<GetWorkloadsItemResponse> Items, Guid? NextCursor);

public record GetWorkloadsItemResponse(Guid Id, Guid CustomerId, string CustomerName, Guid EmployeeId, string EmployeeName, DateTimeOffset Start, DateTimeOffset? Stop, string? Comments, byte[] RowVersion);
