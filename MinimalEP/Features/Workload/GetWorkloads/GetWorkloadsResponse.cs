namespace MinimalEP.Features.Workload.GetWorkloads;

public record GetWorkloadsResponse(IReadOnlyList<GetWorkloadsItemResponse> Items);

public record GetWorkloadsItemResponse(Guid Id, Guid CustomerId, string CustomerName, Guid EmployeeId, string EmployeeName, DateTimeOffset Start, DateTimeOffset? Stop, string? Comments);
