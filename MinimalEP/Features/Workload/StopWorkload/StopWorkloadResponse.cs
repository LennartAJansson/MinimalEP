namespace MinimalEP.Features.Workload.StopWorkload;

public record StopWorkloadResponse(Guid Id, DateTimeOffset Start, DateTimeOffset Stop);
