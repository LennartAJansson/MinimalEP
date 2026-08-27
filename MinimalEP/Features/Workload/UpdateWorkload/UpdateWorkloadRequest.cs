namespace MinimalEP.Features.Workload.UpdateWorkload;

public record UpdateWorkloadRequest(Guid Id, DateTimeOffset Start, DateTimeOffset? Stop, string? Comments);
