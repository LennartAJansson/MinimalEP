namespace MinimalEP.Features.Workload.GetWorkload;

using MinimalEP.Features.Core;

public class GetWorkloadEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapGet(ApiRoutes.Workloads.ById, async (
      Guid id,
      IRequestHandler<GetWorkloadRequest, Result<GetWorkloadResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new GetWorkloadRequest(id), cancellationToken);

      return result.ToHttpResult(ok => TypedResults.Ok(ok));
    }).WithName(ApiRouteNames.GetWorkload);
  }
}
