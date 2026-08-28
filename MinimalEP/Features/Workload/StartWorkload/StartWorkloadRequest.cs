namespace MinimalEP.Features.Workload.StartWorkload;

// Naming mirrors the domain language of a punch clock: you Start a workload, you Stop it later.
// No Stop parameter here by design — this makes it impossible for a client to submit an
// already-closed time entry through this endpoint (an invalid domain state). Compare this to
// "poka-yoke"/"make illegal states unrepresentable" — a Clean Code principle applied via the
// type system itself, rather than via a runtime check.
public record StartWorkloadRequest(Guid CustomerId, DateTimeOffset Start, string? Comments);
