namespace MinimalEP.Features.Workload.UpdateWorkload;

public record UpdateWorkloadRequest(Guid Id, DateTimeOffset Start, string? Comments, byte[] RowVersion);
