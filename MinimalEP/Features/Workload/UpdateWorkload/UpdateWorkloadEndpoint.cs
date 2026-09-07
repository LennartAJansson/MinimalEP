namespace MinimalEP.Features.Workload.UpdateWorkload;

using MinimalEP.Features.Core;

public class UpdateWorkloadEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPut(ApiRoutes.Workloads.ById, async (
      Guid id,
      UpdateWorkloadRequest request,
      IRequestHandler<UpdateWorkloadRequest, Result<UpdateWorkloadResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request with { Id = id }, cancellationToken);

      return result.ToHttpResult(ok => TypedResults.Ok(ok));
    });
  }
}
