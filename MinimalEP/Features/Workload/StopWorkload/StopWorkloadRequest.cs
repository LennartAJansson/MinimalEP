namespace MinimalEP.Features.Workload.StopWorkload;

public record StopWorkloadRequest(Guid Id, DateTimeOffset Stop);
