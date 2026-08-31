namespace MinimalEP.Features.Core;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, Guid? NextCursor);
