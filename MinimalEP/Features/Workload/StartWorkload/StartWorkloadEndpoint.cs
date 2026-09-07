namespace MinimalEP.Features.Workload.StartWorkload;

using MinimalEP.Features.Core;

public class StartWorkloadEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPost(ApiRoutes.Workloads.Start, async (
      StartWorkloadRequest request,
      IRequestHandler<StartWorkloadRequest, Result<StartWorkloadResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request, cancellationToken);

      return result.ToHttpResult(ok => TypedResults.CreatedAtRoute(ok, ApiRouteNames.GetWorkload, new { version = ApiVersions.V1RouteValue, id = ok.Id }));
    });
  }
}
