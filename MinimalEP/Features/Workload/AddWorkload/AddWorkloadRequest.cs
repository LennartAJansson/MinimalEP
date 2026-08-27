namespace MinimalEP.Features.Workload.AddWorkload;

public record AddWorkloadRequest(Guid CustomerId, DateTimeOffset Start, DateTimeOffset? Stop, string? Comments);
