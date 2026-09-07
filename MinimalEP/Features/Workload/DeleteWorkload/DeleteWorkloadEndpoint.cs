namespace MinimalEP.Features.Workload.DeleteWorkload;

using MinimalEP.Features.Core;

public class DeleteWorkloadEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapDelete(ApiRoutes.Workloads.ById, async (
      Guid id,
      IRequestHandler<DeleteWorkloadRequest, Result<Unit>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new DeleteWorkloadRequest(id), cancellationToken);

      return result.ToHttpResult(_ => TypedResults.NoContent());
    });
  }
}
