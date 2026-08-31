namespace MinimalEP.Features.Workload.DeleteWorkload;

using System.Diagnostics;

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

      IResult httpResult = result switch
      {
        Result<Unit>.Ok       => TypedResults.NoContent(),
        Result<Unit>.NotFound => TypedResults.NotFound(),
        _                     => throw new UnreachableException()
      };

      return httpResult;
    });
  }
}
