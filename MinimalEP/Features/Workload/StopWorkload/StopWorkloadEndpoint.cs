namespace MinimalEP.Features.Workload.StopWorkload;

using MinimalEP.Features.Core;

public class StopWorkloadEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPatch(ApiRoutes.Workloads.Stop, async (
      Guid id,
      StopWorkloadRequest request,
      IRequestHandler<StopWorkloadRequest, Result<StopWorkloadResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request with { Id = id }, cancellationToken);

      return result.ToHttpResult(ok => TypedResults.Ok(ok));
    });
  }
}
